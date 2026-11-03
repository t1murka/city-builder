namespace city_builder.Persistence;

public enum DatabaseStatus
{
    Disabled,
    InvalidConfiguration,
    Connecting,
    Ready,
    Unavailable,
    Saving,
    Saved,
    Failed
}
