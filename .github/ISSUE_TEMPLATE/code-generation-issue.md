---
name: Code Generation Issue
about: Report an issue with compiled webservice handlers.
title: ''
labels: bug, code-generation
assignees: ''

---

When using a method with the signature below, the server displays an error page when navigating to the endpoint.

**Example**

```csharp
// paste the signature of the failing method here (the implementation is not required)
var app = Inline.Create()
                .Get(() => 42);
```

**Additional Information**

- [Anything that might be relevant to reproduce this issue]
- [e.g. the server of the version]
- [or what changed so the issue has now occurred]
