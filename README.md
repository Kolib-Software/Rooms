# Kolib Software - Rooms #

- [Room Protocol](#room-protocol)
- [Room Hub Routing](#room-hub-routing)
- [Room Channel Conversion](#room-channel-conversion)
- [Javascript Utils File](./Rooms.Web/wwwroot/rooms.js)
- [Room Test Server](https://kolibsoft-rooms.azurewebsites.net/)

## Room ##

A **Room** is a connection point where participants can send and receive messages. Once connected, a participant has the right to remain in the **Room** and cannot be removed, however the rest of the participants can agree to ignore them. Support TCP and Web Socket based connections.

## Room Protocol #

**Room** is a Websocket subprotocol designed to easily share messages between multiple sockets connected through a central point using UTF8 text:

```txt
// Format:
<ROOM-VERB> <ROOM-CHANNEL> <ROOM-COUNT> [ROOM-CONTENT]

// Example:
MSG 12345678 26 "UTF8 Text or Binary data"
```

- **Room Verb** is a variable length sequence of uppercase or lowercase letters of the ASCII code. The **Room Verb** does not have any special meaning for the hub that relays the messages, it is the member that receive the message that are in charge of interpreting it and how to act in response.

- **Room Channel** is a variable length hexadecimal always signed number that represents a communication channel. The **Room Channel** represents the specific connection point of two members, it is the hub that is responsible for providing the same channel identifier for both parties, this is achieved by performing an XOR operation between the socket identifiers.

- **Room Count** is a variable length unsigned integer number. The **Room Count** represents the content length in bytes.

- **Room Content** is a optional variable length binary data.

## Room Hub Routing ##

When a message is sent to the hub with channel identifier `0` (Hub-Peer) the message with be processed by the hub. It also be used to p2p communication without a hub.

When a message is sent to the hub with the channel identifier `-1` (Broadcast), the hub must broadcast the message to all participants except the author. The channel of the transmitted message in this case must be converted, this is achieved by performing an XOR operation between the sender and the receiver of the message.

When a message is sent to the hub with any identifier other than `0` or `-1` the hub must identify the recipient of the message referenced by the channel to relay it. The receiver's channel identifier can be obtained by performing an XOR operation between the sender's identifier and the channel identifier present in the message.

## Room Channel Conversion ##

In case you want to refer to a specific channel within the body of a message, a conversion of that channel's identifier must be performed, this is achieved by performing an XOR operation between the identifier of the channel that sends the message and the identifier of the channel it refers to within the content of the message.

## Room Console App ##

Command line options:

- `--mode` `[Client, Server]` Allows you to choose between the available modes.
- `--impl` `[TCP, WEB]` Allows you to choose between TCP implementation and Web Sockets implementation.
- `--endpoint` `[localhost:55000]` Allows you to specify the IP endpoint to which you want to connect with the TCP client (by default targets localhost).
- `--uri` `[http://localhost:55000/]` Allows you to specify the URL to which you want to connect with the Web Socket client (by default targets localhost).
- `--settings` `[settings.json]` Allows you to specify the streaming settings file path.
- `--options` `[options.json]` Allows you to specify the connection options file path.

Settings file example:

```json
{
    "StreamOptions":{
        // Read buffer size
        "ReadBuffering": 1024,               /* 1KB by default */
        // Write buffer size
        "WriteBuffering": 1024,              /* 1KB by default */
        // Max verb text representation length
        "MaxVerbLength": 128,                /* by default */
        // Max channel text representation length
        "MaxChannelLength": 32,              /* by default */
        // Max count text representation length
        "MaxCountLength": 32,                /* by default */
        // Max message content length
        "MaxContentLength": 4194304,         /* 4MB by default */
        // Max memory backend content before
        // switch to file backend content
        "MaxFastBuffering": 1048576,         /* 1MB by default */
        // File backend content directory
        "TempContentFolderPath": "Content",  /* by default */
    },
    "ServiceOptions":{
        // Max byte count to send per second before
        // force delay the streaming
        "MaxStreamRate": 1048576 /* 1MB by default */
    }
}
```

Options file example:

```json
{/* Implementation specific (Empty object by default) */}
```

Example of run a TCP Room Server:

```txt
.\Rooms.Console.exe --mode=Server --impl=TCP --endpoint=127.0.0.1:55000 --settings=settings.json --options=options.json
```

Example of run a TCP Room Client:

```txt
.\Rooms.Console.exe --mode=Client --impl=TCP --endpoint=127.0.0.1:55000 --settings=settings.json --options=options.json
```

Example of run a WEB Room Server:

```txt
.\Rooms.Console.exe --mode=Server --impl=WEB --uri=http://localhost:55000/ --settings=settings.json --options=options.json
```

Example of run a WEB Room Client:

```txt
.\Rooms.Console.exe --mode=Client --impl=WEB --uri=ws://localhost:55000/ --settings=settings.json --options=options.json
```
