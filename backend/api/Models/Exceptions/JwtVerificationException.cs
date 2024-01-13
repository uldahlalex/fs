namespace api.Models.Exceptions;

public class JwtVerificationException(string message) : Exception(message);