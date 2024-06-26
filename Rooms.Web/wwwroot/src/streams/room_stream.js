import { RoomChannel } from "../protocol/room_channel.js";
import { RoomCount } from "../protocol/room_count.js";
import { RoomDataUtils, encoder } from "../protocol/room_data_utils.js";
import { RoomMessage } from "../protocol/room_message.js";
import { RoomVerb } from "../protocol/room_verb.js";
import { MemoryStream } from "./memory_stream.js";
import { RoomStreamOptions } from "./room_stream_options.js";

const BLANK = encoder.encode(" ");

class RoomStream {

    #options;
    #data;
    #readBuffer;
    #writeBuffer;
    #position;
    #length;
    #disposed;

    get options() { return this.#options; }
    get isAlive() { throw Error("Abstract member"); }
    get isDisposed() { return this.#disposed; }

    async readAsync(chunk) { throw Error("Abstract member"); }

    async #getChunkAsync() {
        if (this.#position == this.#length) {
            this.#position = 0;
            this.#length = await this.readAsync(this.#readBuffer);
            if (this.#length < 1)
                return new Uint8Array(0);
        }
        let slice = RoomDataUtils.slice(this.#readBuffer, this.#position, this.#length - this.#position);
        return slice;
    }

    async #readVerbAsync() {
        this.#data.position = 0;
        this.#data.length = 0;
        let done = false;
        while (true) {
            let chunk = await this.#getChunkAsync();
            if (this.#length < 1) throw new Error("Room verb broken");
            let length = RoomDataUtils.scanWord(chunk);
            if (length < chunk.length)
                length += (done = RoomDataUtils.isBlank(chunk[length])) ? 1 : 0;
            this.#position += length;
            if (this.#data.length + length > this.#options.maxVerbLength) throw new Error("Room verb too large");
            if (this.#position < length || done) {
                if (this.#data.length > 0) {
                    await this.#data.writeAsync(RoomDataUtils.slice(chunk, 0, length - 1));
                    let verb = new RoomVerb(this.#data.toArray());
                    return verb;
                }
                if (length > 0) {
                    let verb = new RoomVerb(chunk.slice(0, length - 1));
                    return verb;
                }
                return new RoomVerb();
            }
            await this.#data.writeAsync(chunk);
        }
    }

    async #readChannelAsync() {
        this.#data.position = 0;
        this.#data.length = 0;
        let done = false;
        while (true) {
            let chunk = await this.#getChunkAsync();
            if (this.#length < 1) throw new Error("Room channel broken");
            let length = RoomDataUtils.isSign(chunk[0]) ? 1 : 0;
            if (length < chunk.length)
                length += RoomDataUtils.scanHexadecimal(RoomDataUtils.slice(chunk, length));
            if (length < chunk.length)
                length += (done = RoomDataUtils.isBlank(chunk[length])) ? 1 : 0;
            this.#position += length;
            if (this.#data.length + length > this.#options.maxChannelLength) throw new Error("Room channel too large");
            if (this.#position < length || done) {
                if (this.#data.length > 0) {
                    await this.#data.writeAsync(RoomDataUtils.slice(chunk, 0, length - 1));
                    let channel = new RoomChannel(this.#data.toArray());
                    return channel;
                }
                if (length > 0) {
                    let channel = new RoomChannel(chunk.slice(0, length - 1));
                    return channel;
                }
                return new RoomChannel();
            }
            await this.#data.writeAsync(chunk);
        }
    }

    async #readCountAsync() {
        this.#data.position = 0;
        this.#data.length = 0;
        let done = false;
        while (true) {
            let chunk = await this.#getChunkAsync();
            if (this.#length < 1) throw new Error("Room count broken");
            let length = RoomDataUtils.scanDigit(chunk);
            if (length < chunk.length)
                length += (done = RoomDataUtils.isBlank(chunk[length])) ? 1 : 0;
            this.#position += length;
            if (this.#data.length + length > this.#options.maxCountLength) throw new Error("Room count too large");
            if (this.#position < length || done) {
                if (this.#data.length > 0) {
                    await this.#data.writeAsync(RoomDataUtils.slice(chunk, 0, length - 1));
                    let count = new RoomCount(this.#data.toArray());
                    return count;
                }
                if (length > 0) {
                    let count = new RoomCount(chunk.slice(0, length - 1));
                    return count;
                }
                return new RoomCount();
            }
            await this.#data.writeAsync(chunk);
        }
    }

    async #readContentAsync() {
        let count = await this.#readCountAsync();
        let _count = parseInt(count);
        if (_count == 0) return {};
        if (_count > this.#options.maxContentLength) throw new Error("Room content too large");
        let content = new MemoryStream();
        let index = 0;
        while (index < _count) {
            let chunk = await this.#getChunkAsync();
            if (this.#length < 1) throw new Error("Room content broken");
            let length = Math.min(chunk.length, _count - index);
            await content.writeAsync(RoomDataUtils.slice(chunk, 0, length));
            index += length;
            this.#position += length;
        }
        return content.toArray();
    }

    async readMessageAsync() {
        if (this.#disposed) throw new Error("RoomStream was disposed");
        let verb = await this.#readVerbAsync();
        let channel = await this.#readChannelAsync();
        let content = await this.#readContentAsync();
        let message = new RoomMessage({
            verb: verb.toString(),
            channel: parseInt(channel, 16),
            content
        });
        return message;
    }

    async writeAsync(chunk) { throw Error("Abstract member"); }

    async #writeVerbAsync(verb) {
        if (!(verb instanceof RoomVerb)) throw new Error("Invalid argument");
        if (verb.length > this.#options.maxVerbLength) throw new Error("Room verb too large");
        let index = 0;
        while (index < verb.length) {
            let length = await this.writeAsync(RoomDataUtils.slice(verb.data, index));
            if (length < 1) throw new Error("Room verb broken");
            index += length;
        }
        await this.writeAsync(BLANK);
    }

    async #writeChannelAsync(channel) {
        if (!(channel instanceof RoomChannel)) throw new Error("Invalid argument");
        if (channel.length > this.#options.maxChannelLength) throw new Error("Room channel too large");
        let index = 0;
        while (index < channel.length) {
            let length = await this.writeAsync(RoomDataUtils.slice(channel.data, index));
            if (length < 1) throw new Error("Room channel broken");
            index += length;
        }
        await this.writeAsync(BLANK);
    }

    async #writeCountAsync(count) {
        if (!(count instanceof RoomCount)) throw new Error("Invalid argument");
        if (count.length > this.#options.maxChannelLength) throw new Error("Room count too large");
        let index = 0;
        while (index < count.length) {
            let length = await this.writeAsync(RoomDataUtils.slice(count.data, index));
            if (length < 1) throw new Error("Room count broken");
            index += length;
        }
        await this.writeAsync(BLANK);
    }

    async #writeContentAsync(content) {
        if (!(content instanceof Uint8Array)) throw new Error("Invalid argument");
        if (content.length > this.#options.maxContentLength) throw new Error("Room content too large");
        let count = new RoomCount(content.length);
        await this.#writeCountAsync(count);
        let index = 0;
        while (index < content.length) {
            this.#writeBuffer.set(content);
            let _count = content.length;
            let slice = RoomDataUtils.slice(this.#writeBuffer, 0, _count);
            let _index = 0;
            while (_index < slice.length) {
                let length = await this.writeAsync(RoomDataUtils.slice(slice, _index));
                if (length < 1) throw new Error("Room content broken");
                _index += length;
            }
            index += slice.length;
        }
    }

    async writeMessageAsync(message) {
        if (!(message instanceof RoomMessage)) throw new Error("Invalid argument");
        if (this.#disposed) throw new Error("RoomStream was disposed");
        let verb = RoomVerb.parse(message.verb);
        let channel = new RoomChannel(message.channel);
        let content = message.content;
        await this.#writeVerbAsync(verb);
        await this.#writeChannelAsync(channel);
        await this.#writeContentAsync(content);
    }

    async onDisposeAsync(disposing) {
        if (!this.#disposed) {
            if (disposing)
                this.#data.length = 0;
            this.#readBuffer = null;
            this.#writeBuffer = null;
            this.#disposed = true;
        }
    }

    dispose() { let _ = this.onDisposeAsync(true); }
    async disposeAsync() { await this.onDisposeAsync(true); }

    constructor(args = { readBuffer: null, writeBuffer: null, options: null }) {
        if (args?.readBuffer && !(args.readBuffer instanceof Uint8Array) || args?.writeBuffer && !(args.writeBuffer instanceof Uint8Array))
            throw new Error("Invalid arguments");
        this.#options = new RoomStreamOptions(args?.options);
        this.#readBuffer = args?.readBuffer ?? new Uint8Array(this.#options.readBuffering);
        this.#writeBuffer = args?.writeBuffer ?? new Uint8Array(this.#options.writeBuffering);
        this.#data = new MemoryStream();
        this.#position = 0;
        this.#length = 0;
        this.#disposed = false;
    }

}

export {
    RoomStream
}