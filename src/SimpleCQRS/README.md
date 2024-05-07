# Teast.SimpleCQRS

This is a simple "CQRS"/EventStore implementation that can be used to simplify the boiling plates when building such solutions.

## Implementation

The idea is to have `Command` objects that drives changes to your entity.
One implementation is to create an `CommandHandler` class that can handle your `Commands` and that uses your `Aggregate` to generate an corresponding `Event` object.

### Difference between Command and Event

The main different between this two objects is that a `Command` should be validated and can be rejected while an `Event` should never be validated and instead always be applied.
A `Command` that is accepted should generate an `Event` and the `Event` is the one that should be stored in your data storage and then later on read from data storage when you re-play your entity through an `Aggregate`

### Difference between snapshot and projection

a projection should always represent latest state of your entity while a snapshot can be from any state of your entity.

### IStorage

This interface is the glue between Simple.CQRS and your data storage.
When `Repository` needs to fetch events, snapshot, versions, etc from the data storage it will use an object that implements this interface.

### Repository

This abstract class will take care of extracting changes from an existing `Aggregate` for saving to data storage or to create an `Aggregate` based of data fetched from `IStorage`.

### Aggregate

Aggregate handles creation of events and to load events.

### Event

Represents an change to your entity. All changes that you support should inherit from `Event`

### Data

This class represents an snapshot (or projection) of your entity.
It is possible to use SimpleCQRS without having an dedicated snapshot class.
