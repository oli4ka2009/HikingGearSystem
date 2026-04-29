# Project Architectural Rules

1. **Modern Syntax**: Use ONLY modern Angular 17+ and RxJS 7+ syntax.
2. **RxJS Imports**: No imports from `'rxjs/operators'`. All operators must be imported exclusively from `'rxjs'`.
3. **Angular Standards**:
    - Use **Standalone components**.
    - Use the **new Control Flow** (`@if`, `@for`, `@switch`).
    - Use the **`inject()` function** for dependency injection.
    - Do not generate legacy or deprecated code.
