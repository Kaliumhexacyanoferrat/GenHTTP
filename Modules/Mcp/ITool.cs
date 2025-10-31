namespace GenHTTP.Modules.Mcp;

public interface ITool
{

    string Name { get; }

    string Description { get; }

    internal Type InputType { get; }

    internal Type OutputType { get; }

    internal object CallUntyped(object input);

}

public interface ITool<in TInput, out TOutput> : ITool
{

    TOutput Call(TInput input);

    Type ITool.InputType => typeof(TInput);

    Type ITool.OutputType => typeof(TOutput);

    object ITool.CallUntyped(object input) => Call(((TInput)input)!)!;

}
