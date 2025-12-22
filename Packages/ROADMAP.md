### Remove Lobby facade
- Remove the static Lobby facade and rely on DI for injection instead.
- Remove the idea of pre-init calls, this just hides errors. Still useful to have null objects for safe no-ops after init.

### Providers cannot use Lobby API during init
- If providers use the lobby API during construction or init, they will fail because the lobby is not marked ready yet.
- This is sort of superficial because the services in the Lobby are correctly set at this point, but it feels bad. May resolve when removing facade though.