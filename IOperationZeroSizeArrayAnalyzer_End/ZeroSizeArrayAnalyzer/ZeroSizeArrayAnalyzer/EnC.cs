#if DEBUG
namespace System.Runtime.CompilerServices
{
    class AsyncStateMachineAttribute : StateMachineAttribute { public AsyncStateMachineAttribute(Type type) : base(type) { } }
    class IteratorStateMachineAttribute : StateMachineAttribute { public IteratorStateMachineAttribute(Type type) : base(type) { } }
}
#endif

