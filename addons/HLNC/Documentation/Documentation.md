Documentation

### Tick-alignment

Every "Tick", the server serializes all networkable game data, bundles it, and sends it to the clients.

### Server State Authority

The server is the ultimate source of truth for the entire game state.

### Input Authority

The server determines what clients are allowed to send inputs, and for what objects. This prevents hacking, and ensures that users cannot make cheats / illegal actions.

#### What exactly does this mean?
The complexity of this is completely abstracted away by HLNC, so the implementation details don't really matter for practical purposes.

In short, the client informs the server of their intended _verbs_, and it is up to the server to ultimately decide how that mutates the state.