using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace PricingService.Configuration;

public static class ExceptionMapper
{
    public static Task WriteExceptionResponseAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return context.Response.WriteAsync(SerializeValidationException(validationException));
        }

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        return context.Response.WriteAsync(JsonConvert.SerializeObject(new
        {
            Message = "An error occured whilst processing your request"
        }));
    }

    private static string SerializeValidationException(ValidationException ex)
    {
        return JsonConvert.SerializeObject(new
        {
            Code = "400",
            Message = ex.Errors.ToString()
        });
    }
}
