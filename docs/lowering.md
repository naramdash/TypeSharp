# Lowering Examples

문서 기준일: 2026-05-19

이 문서는 현재 구현된 TypeSharp MVP 기능이 C# 7.3-compatible source backend를 통해 어떤 `net48` C# 코드로 낮아지는지 설명한다. 목표는 새 기능을 추가할 때 "어떤 C# source, 어떤 runtime helper, 어떤 public metadata shape, 어떤 검증으로 낮아지는가"를 빠르게 확인할 수 있게 하는 것이다.

## Backend Contract

현재 MVP backend는 직접 IL emit이 아니라 deterministic C# source generation이다.

필수 계약:
- generated project는 SDK-style `net48` C# project다.
- generated C# source는 C# 7.3 compiler가 수용해야 한다.
- generated public API는 C# `net48` consumer가 일반 assembly reference로 호출할 수 있어야 한다.
- TypeSharp compile-time-only type-level union과 structural shape는 public CLR metadata로 직접 노출하지 않는다.
- runtime helper가 필요한 lowering은 `TypeSharp.Runtime`의 package-free `net48` API에만 의존한다.

## Lowering Pipeline

현재 compiler는 backend emit 직전에 `TypeSharp.Compiler.Lowering.TypeSharpLoweringPipeline.Default`를 실행한다.

Pipeline contract:
- pass는 순서가 명시된 `ITypeSharpLoweringPass`로 구성한다.
- pass는 같은 syntax tree에 반복 적용해도 duplicate helper/import를 만들지 않는 idempotent 동작이어야 한다.
- pass 결과는 deterministic해야 하며 C# source backend output stability를 깨면 안 된다.

Implemented pass:
- `csharp-runtime-import`: union declaration or match expression이 runtime helper를 필요로 할 때 synthetic `TypeSharp.Runtime` import를 추가한다.

주요 검증:
- `C# backend fixture snapshots match`
- `lowering pipeline injects runtime helper imports`
- `generated C# compiles in net48 project`
- `CLI build emits generated net48 assembly`
- `C# net48 project consumes generated TypeSharp assembly`
- `net48 application model hosts reference generated assembly and runtime`

## Function And Module

TypeSharp:

```tysh
namespace Samples.Backend

export fun greeting(): string = "Hello"
```

Generated C#:

```csharp
namespace Samples.Backend
{
    public static class Module
    {
        public static string greeting()
        {
            return "Hello";
        }
    }
}
```

Rules:
- top-level functions lower to static methods on generated `Module`.
- `export` lowers to `public`; private/internal source remains non-public.
- expression-bodied functions lower to block-bodied C# methods with `return`.

Evidence:
- `tests/fixtures/backend/csharp/positive/0001-string-return`
- `tests/fixtures/backend/csharp/positive/0008-basic-semantics`
- `CLI build compiles basic semantics`

## Imports And Calls

TypeSharp:

```tysh
import { Regex } from "System.Text.RegularExpressions"

export fun valid(input: string): bool = Regex.IsMatch(input, "^[a-z]+$")
```

Generated C#:

```csharp
using System.Text.RegularExpressions;

return Regex.IsMatch(input, "^[a-z]+$");
```

Rules:
- named imports lower to C# `using` directives.
- static imports lower to `using static`.
- TypeSharp member access and call syntax is preserved as C# member access and call syntax.
- manifest references are propagated to the generated C# project as `<Reference>` items.

Evidence:
- `tests/fixtures/backend/csharp/positive/0002-import-directives`
- `tests/fixtures/backend/csharp/positive/0003-call-expression`
- `CLI build compiles framework static member call`
- `CLI build compiles local DLL static member call`
- `CLI build propagates manifest references to generated C# project`

## Blocks And Locals

TypeSharp:

```tysh
export fun greet(name: string): string {
  let prefix = "Hello, "
  prefix + name
}
```

Generated C#:

```csharp
public static string greet(string name)
{
    var prefix = "Hello, ";
    return prefix + name;
}
```

Rules:
- local `let` lowers to `var`.
- the last expression in a block lowers to `return`.
- earlier expression statements are emitted as normal C# statements when supported.

Evidence:
- `tests/fixtures/backend/csharp/positive/0004-block-local`
- `tests/fixtures/backend/csharp/positive/0008-basic-semantics`

## Pipeline Expressions

TypeSharp:

```tysh
export fun compute(): string =
  1
  |> increment
  |> add(2)
  |> format
```

Generated C#:

```csharp
public static string compute()
{
    return format(add(increment(1), 2));
}
```

Rules:
- `value |> f` lowers to `f(value)`.
- `value |> f(args...)` lowers to `f(value, args...)`.
- chained pipelines lower as left-associative nested calls.
- placeholder-based partial application and dedicated composition operators remain Stable Backlog.

Evidence:
- `tests/fixtures/backend/csharp/positive/0023-pipeline-lowering`
- `CLI build compiles pipeline lowering`

## Literals

TypeSharp:

```tysh
public literal ApiVersion: string = "1.0"
literal MaxRetryCount = 3
```

Generated C#:

```csharp
public const string ApiVersion = "1.0";
internal const int MaxRetryCount = 3;
```

Rules:
- primitive compile-time literals lower to C# `const` when C# supports the literal kind.
- non-const literal-compatible declarations lower to `static readonly`.
- public literals are callable from C# consumers as normal fields.

Evidence:
- `tests/fixtures/backend/csharp/positive/0007-literal-constants`
- `CLI build compiles literal constants`

## Collection Expressions

TypeSharp:

```tysh
export fun names(): string[] = ["Ada", "Grace"]

export fun numbers(): int[] {
  let values: int[] = [1, 2, 3]
  values
}

export fun nameList(): List<string> = ["Ada", "Grace"]
```

Generated C#:

```csharp
public static string[] names()
{
    return new string[] { "Ada", "Grace" };
}

public static int[] numbers()
{
    var values = new int[] { 1, 2, 3 };
    return values;
}

public static List<string> nameList()
{
    return new List<string> { "Ada", "Grace" };
}
```

Rules:
- simple homogeneous collection expressions lower to C# 7.3-compatible array creation expressions.
- explicit target `List<T>` collection expressions lower to C# 7.3-compatible collection initializers.
- expected array types from return annotations or local annotations drive empty array lowering.
- expected `List<T>` types from return annotations or local annotations drive empty list lowering.
- mixed known element types are reported as `TS2201`.
- target-type-free `List<T>` inference, dictionary, spread, target-specific builder, and advanced C# collection expression forms remain Stable Backlog.

Evidence:
- `tests/fixtures/backend/csharp/positive/0022-collection-expression-lowering`
- `tests/fixtures/diagnostics/type-checker/negative/collection-expression-mismatch`
- `CLI build compiles collection expression lowering`

## Indexer Expressions

TypeSharp:

```tysh
export fun firstName(): string {
  let names: string[] = ["Ada", "Grace"]
  names[0]
}
```

Generated C#:

```csharp
public static string firstName()
{
    var names = new string[] { "Ada", "Grace" };
    return names[0];
}
```

Rules:
- `receiver[index]` lowers to C# 7.3-compatible indexer or array access.
- array receiver types such as `T[]` infer the indexed expression as `T`.
- imported C# indexers are preserved as normal generated C# indexer access.
- richer metadata-backed indexer overload validation remains future work.

Evidence:
- `tests/fixtures/backend/csharp/positive/0024-indexer-expression-lowering`
- `CLI build compiles imported indexer access`

## Records

TypeSharp:

```tysh
public record Customer(Name: string, Age: int)

export fun create(): Customer = { Name: "Ada", Age: 36 }

export fun birthday(customer: Customer): Customer =
  customer with { Age: 43 }
```

Generated C# shape:

```csharp
public sealed class Customer
{
    public Customer(string Name, int Age) { ... }
    public string Name { get; }
    public int Age { get; }
    public override bool Equals(object obj) { ... }
    public override int GetHashCode() { ... }
}

return new Customer("Ada", 36);
return new Customer(customer.Name, 43);
```

Rules:
- TypeSharp `record` lowers to a sealed immutable C# class.
- constructor parameters become get-only public properties.
- value equality and hash code are emitted explicitly for C# 7.3 compatibility.
- expected nominal record expressions lower to constructor calls in record parameter order.
- record expression fields are type-checked against the target record or structural shape when the expected type is known.
- record update lowers to a constructor call that reads unchanged fields from the source record.
- structural object literal lowering without a nominal target remains future work.

Evidence:
- `tests/fixtures/backend/csharp/positive/0015-immutable-record-api`
- `tests/fixtures/backend/csharp/positive/0016-record-update-lowering`
- `tests/fixtures/backend/csharp/positive/0025-record-expression-construction`
- `tests/fixtures/diagnostics/type-checker/negative/record-expression-mismatch`
- `CLI build compiles immutable record API`
- `CLI build compiles record update lowering`
- `CLI build compiles record expression construction`

## Public API Declarations

TypeSharp class/interface/generic declarations lower to matching C# class, interface, and generic method/type declarations when they are nominal and CLR-representable.

Evidence:
- `tests/fixtures/backend/csharp/positive/0011-generic-function-api`
- `tests/fixtures/backend/csharp/positive/0012-class-declaration-api`
- `tests/fixtures/backend/csharp/positive/0013-interface-declaration-api`
- `tests/fixtures/backend/csharp/positive/0014-generic-type-declaration-api`
- `CLI build compiles generic function API`
- `CLI build compiles class declaration API`
- `CLI build compiles interface declaration API`
- `CLI build compiles generic type declaration API`

## Nominal Union

TypeSharp:

```tysh
public union PaymentStatus {
  Pending
  Paid(receipt: string)
}
```

Generated C# shape:

```csharp
public abstract class PaymentStatus
{
    public static PaymentStatus Pending { get; }
    public static PaymentStatus Paid(string receipt) { ... }

    public sealed class PaidCase : PaymentStatus, ITypeSharpUnionCase { ... }
}
```

Rules:
- nominal unions lower to an abstract base type with sealed nested case types.
- payload-free cases expose static properties.
- payload cases expose factory methods.
- generated case classes implement `ITypeSharpUnionCase` so runtime pattern helpers can inspect tag, case name, and payload.

Evidence:
- `tests/fixtures/backend/csharp/positive/0017-nominal-union-api`
- `CLI build compiles nominal union API`

## Pattern Matching

TypeSharp:

```tysh
export fun describe(status: PaymentStatus): string =
  match status {
    Pending => "pending"
    Paid(receipt) => receipt
  }
```

Generated C# shape:

```csharp
var __match0 = status;
if (TypeSharpPattern.IsPayloadlessCase(__match0, 0)) { ... }
if (TypeSharpPattern.IsPayloadCase(__match0, 1)) { ... }
throw TypeSharpPattern.NoMatch(__match0);
```

Rules:
- nominal union match lowers to ordered C# case checks.
- payload extraction uses `TypeSharpPattern.RequirePayload<T>`.
- type checker reports non-exhaustive nominal union matches before backend emission.

Evidence:
- `tests/fixtures/backend/csharp/positive/0018-nominal-union-match-lowering`
- `tests/fixtures/diagnostics/type-checker/negative/non-exhaustive-union-match`
- `CLI build compiles nominal union match lowering`
- `CLI build stops before emission on non-exhaustive match`

## Type-Level Union Narrowing

TypeSharp:

```tysh
type PrimitiveId = string | int

fun normalize(id: PrimitiveId): string =
  match id {
    text: string => text
    number: int => number.ToString()
  }
```

Generated C# shape:

```csharp
internal static string normalize(object id)
{
    var __match0 = id;
    if (__match0 is string text) { return text; }
    if (__match0 is int number) { return number.ToString(); }
    throw TypeSharpPattern.NoMatch(__match0);
}
```

Rules:
- type-level unions are compile-time-only and erase to `object` in local generated signatures.
- type-pattern match arms lower to C# `is Type name` checks.
- public APIs cannot expose type-level union aliases directly; the type checker reports `TS2204`.

Evidence:
- `tests/fixtures/backend/csharp/positive/0019-type-level-union-narrowing`
- `tests/fixtures/diagnostics/type-checker/negative/public-boundary-union-alias`
- `CLI build compiles type-level union narrowing`
- `CLI check emits JSON public boundary diagnostics`

## Structural Shapes

TypeSharp structural shapes are currently compile-time-only.

TypeSharp:

```tysh
type Named = { Name: string }
record Person(Name: string, Age: int)

fun label(value: Named): string = value.Name
```

Rules:
- local structural aliases participate in type checking.
- nominal records can satisfy a structural alias when required members are present and compatible.
- structural shapes do not lower to public CLR metadata.
- public APIs cannot expose structural shape aliases directly; the type checker reports `TS2204`.

Evidence:
- `tests/fixtures/diagnostics/type-checker/positive/structural-shape`
- `tests/fixtures/diagnostics/type-checker/negative/structural-shape-mismatch`
- `CLI check emits JSON structural diagnostics`

## Async Task Interop

TypeSharp:

```tysh
import { Task } from "System.Threading.Tasks"

export async fun greeting(): Task<string> {
  let value = await Task.FromResult("Hello")
  value
}
```

Generated C#:

```csharp
using System.Threading.Tasks;

public static async Task<string> greeting()
{
    var value = await Task.FromResult("Hello");
    return value;
}
```

Rules:
- `async fun` lowers to a C# `async` method.
- `await` lowers to C# `await`.
- public async APIs must currently spell out `Task` or `Task<T>`.
- richer task inference, async `main`, and byref async-boundary diagnostics remain future work.

Evidence:
- `tests/fixtures/backend/csharp/positive/0020-async-task-interop`
- `CLI build compiles async Task interop`

## Unsupported Or Compile-Time-Only Lowering

The following constructs are intentionally not emitted as direct public CLR metadata in the MVP:

- type-level union aliases
- structural shape aliases
- anonymous structural object types
- `unknown`
- marker-free `dynamic` escape

The current rule is to keep these constructs local to TypeSharp checking or require a nominal wrapper, nominal union, or interface before crossing a public .NET boundary.

Evidence:
- `tests/fixtures/diagnostics/type-checker/negative/public-boundary-union-alias`
- `docs/csharp-interop.md`
- `docs/runtime-abi.md`
