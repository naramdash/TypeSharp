# pipeline-function-input-compatibility

Verifies direct `value |> f` and `value |> f(args...)` targets report `TS2215` when a known TypeSharp-declared first parameter cannot accept the pipeline input. Compatible direct targets remain accepted.
