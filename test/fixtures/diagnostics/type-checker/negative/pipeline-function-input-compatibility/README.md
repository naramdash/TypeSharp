# pipeline-function-input-compatibility

Verifies direct `value |> f` and `value |> f(args...)` targets report `TS2201` when a known TypeSharp-declared first parameter cannot accept the pipeline input. Compatible direct targets and zero-parameter targets remain outside this bounded diagnostic.
