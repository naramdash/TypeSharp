# Extension property helper-name collisions

Ensures getter-only TypeSharp-authored extension properties report deterministic
diagnostics when their generated `GetName` helper collides with an extension
method in the same extension container.
