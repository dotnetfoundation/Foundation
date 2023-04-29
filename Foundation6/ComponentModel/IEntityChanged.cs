﻿namespace Foundation.ComponentModel;

public interface IEntityChanged<TEventId, TEntityId, TChangedState> : IEntityEvent<TEventId, TEntityId>
    where TEntityId : notnull
{
    TChangedState ChangedState { get; }
}

public interface IEntityChanged<TEventId, TObjectType, TEntityId, TChangedState>
    : IEntityChanged<TEventId, TEntityId, TChangedState>
    , ITypedObject<TObjectType>
    where TEntityId : notnull
{
}
