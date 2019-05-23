C# Coding Style
===============

The style is similar to [dotnet/corefx](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md) code style except for field prefixes. The general rule to follow is "use Visual Studio defaults".

1. Use [Allman style](http://en.wikipedia.org/wiki/Indent_style#Allman_style) braces, where each brace begins on a new line. A single line statement block can go without braces but the block must be properly indented on its own line and must not be nested in other statement blocks that use braces. One exception is that a `using` statement is permitted to be nested within another `using` statement by starting on the following line at the same indentation level, even if the nested `using` contains a controlled block.
2. Use four spaces of indentation (no tabs).
3. Use `camelCase` for internal and private fields and use `readonly` where possible. Do not prefix any fields. When used on static fields, `readonly` should come after `static` (e.g. `static readonly` not `readonly static`).  Public fields should be used sparingly and should use `PascalCase` with no prefix when used.
4. Use `PascalCase` to name all constant local variables and fields.
5. Public (auto) properties are preferred over public fields except in simple data structs. Use `PascalCase` for properties.
6. Use `IPascalCase` for interfaces and `TPascalCase` for type parameters. Use `PascalCase` for all other types and members.
7. Avoid `this.` unless absolutely necessary. 
8. Always specify the visibility, even if it's the default (e.g.
   `private string foo` not `string foo`). Visibility should be the first modifier (e.g. 
   `public abstract` not `abstract public`).
9.  Namespace imports should be specified at the top of the file, *outside* of
   `namespace` declarations, and should be sorted alphabetically, with the exception of `System.*` namespaces, which are to be placed on top of all others.
11. Avoid more than one empty line at any time. For example, do not have two
   blank lines between members of a type.
11. Avoid spurious free spaces.
   For example avoid `if (someVar == 0)...`, where the dots mark the spurious free spaces.
   Consider enabling "View White Space (Ctrl+E, S)" if using Visual Studio to aid detection.
11. Only use `var` when it's obvious what the variable type is (e.g. `var stream = new FileStream(...)` not `var stream = OpenStandardInput()`).
12. Use language keywords instead of BCL types (e.g. `int, string, float` instead of `Int32, String, Single`, etc) for both type references as well as method calls (e.g. `int.Parse` instead of `Int32.Parse`). 
13. Use ```nameof(...)``` instead of ```"..."``` whenever possible and relevant.
14. Keep similar type members together. General file layout is:
    1. Public delegates
    2. Public enums
    3. Static fields and constants
    4. Fields
    5. Constructors
    6. Properties, indexers
    7. Interface implementations
    8. All other members
    9. Nested types
15. When including non-ASCII characters in the source code use Unicode escape sequences (\uXXXX) instead of literal characters. Literal non-ASCII characters occasionally get garbled by a tool or editor.
16. When using labels (for `goto`), indent the label one less than the current indentation.
17. Keep lines of code reasonably long (ReSharper is set to wrap at 120 characters), wrap longer lines. This way the code fits inside the editor window and it makes inspecting diffs easier.

An [EditorConfig](https://editorconfig.org "EditorConfig homepage") file (`.editorconfig`) has been provided at the root of the repository, enabling C# auto-formatting conforming to the above guidelines.
