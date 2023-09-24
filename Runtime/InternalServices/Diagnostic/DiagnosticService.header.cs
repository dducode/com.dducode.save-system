namespace SaveSystem.InternalServices.Diagnostic {

    internal static partial class DiagnosticService {

        internal static partial void AddMetadata (HandlerMetadata metadata);
        internal static partial void AddObject (int index);
        internal static partial void AddObjects (int index, int count);
        internal static partial void RemoveObject (int index);

    }

}