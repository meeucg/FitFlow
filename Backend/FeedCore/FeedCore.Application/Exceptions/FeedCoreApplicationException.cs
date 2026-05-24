namespace FeedCore.Application.Exceptions;

public abstract class FeedCoreApplicationException(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class FeedCoreValidationException(string message)
    : FeedCoreApplicationException(message);

public sealed class FeedCoreNotFoundException(string message)
    : FeedCoreApplicationException(message);

public sealed class EmbeddingProviderException(string message, Exception? innerException = null)
    : FeedCoreApplicationException(message, innerException);

public sealed class FeedCorePersistenceException(string message, Exception? innerException = null)
    : FeedCoreApplicationException(message, innerException);
