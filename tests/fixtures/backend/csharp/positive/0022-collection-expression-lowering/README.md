# C# Backend Positive: Collection Expression Lowering

Purpose: verifies that simple homogeneous collection expressions lower to C# 7.3-compatible array creation expressions by default and to `List<T>` collection initializers when the target type is explicit.
