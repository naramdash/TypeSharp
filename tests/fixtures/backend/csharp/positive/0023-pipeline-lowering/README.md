# C# Backend Positive: Pipeline Lowering

Purpose: verifies that `value |> f` and `value |> f(args...)` lower to nested C# calls with the piped value inserted as the first argument.
