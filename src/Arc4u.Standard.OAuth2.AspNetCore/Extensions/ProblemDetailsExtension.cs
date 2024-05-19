using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Arc4u.Results;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.HttpResults;
//using Microsoft.AspNetCore.Http;

namespace Arc4u.OAuth2.AspNetCore.Extensions;
public static class ProblemDetailsExtension
{
    #region ActionResult

    // this ValueTask<Result<T>>
    public static async ValueTask<ActionResult<T>> ToActionOkResultAsync<TResult, T>(this ValueTask<Result<TResult>> result, Func<TResult, T>? mapper = null, Func<StatusCodeResult>? nullCode = null)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult<T> objectResult = new BadRequestResult();
        res
            .OnSuccess(value => objectResult = new OkObjectResult(mapper is null ? res.Value : mapper(res.Value!)))
            .OnSuccessNull(() => objectResult = null == nullCode ? new OkObjectResult(null) : nullCode())
            .OnFailed(errors => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<ActionResult<TResult>> ToActionOkResultAsync<TResult>(this ValueTask<Result<TResult>> result, Func<StatusCodeResult>? nullCode = null)
    {
        return await ToActionOkResultAsync<TResult,TResult>(result, null, nullCode).ConfigureAwait(false);
    }

    public static async ValueTask<ActionResult<T>> ToActionCreatedResultAsync<TResult, T>(this ValueTask<Result<TResult>> result, Uri? location, Func<TResult, T>? mapper = null, Func<StatusCodeResult>? nullCode = null)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult<T> objectResult = new BadRequestResult();
        res
#if NET8_0
            .OnSuccess(value => objectResult = new CreatedResult(location, mapper is null ? value : mapper(value!)))
#else
            .OnSuccess(value =>
            {
                if (location is null)
                {
                    objectResult = new ObjectResult(mapper is null ? value : mapper(value!))
                    {
                        StatusCode = StatusCodes.Status201Created
                    };
                }
                else
                {
                    objectResult = new CreatedResult(location, mapper is null ? value : mapper(value!));
                }
            })
                                                                
#endif
            .OnSuccessNull(() => objectResult = null == nullCode ? new OkObjectResult(null) : nullCode())
            .OnFailed(errors => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<ActionResult<TResult>> ToActionCreatedResultAsync<TResult>(this ValueTask<Result<TResult>> result, Uri? location, Func<StatusCodeResult>? nullCode = null)
    {
        return await ToActionCreatedResultAsync<TResult, TResult>(result, location, null, nullCode).ConfigureAwait(false);
    }

    // this Task<Result<T>>
    public static async ValueTask<ActionResult<T>> ToActionOkResultAsync<TResult, T>(this Task<Result<TResult>> result, Func<TResult, T>? mapper = null, Func<StatusCodeResult>? nullCode = null)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult<T> objectResult = new BadRequestResult();
        res
            .OnSuccess(value => objectResult = new OkObjectResult(mapper is null ? res.Value : mapper(res.Value!)))
            .OnSuccessNull(() => objectResult = null == nullCode ? new OkObjectResult(null) : nullCode())
            .OnFailed(errors => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<ActionResult<TResult>> ToActionOkResultAsync<TResult>(this Task<Result<TResult>> result, Func<StatusCodeResult>? nullCode = null)
    {
        return await ToActionOkResultAsync<TResult, TResult>(result, null, nullCode).ConfigureAwait(false);
    }

    public static async ValueTask<ActionResult<T>> ToActionCreatedResultAsync<TResult, T>(this Task<Result<TResult>> result, Uri? location, Func<TResult, T>? mapper = null, Func<StatusCodeResult>? nullCode = null)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult<T> objectResult = new BadRequestResult();
        res
#if NET8_0
            .OnSuccess(value => objectResult = new CreatedResult(location, mapper is null ? value : mapper(value!)))
#else
            .OnSuccess(value =>
            {
                if (location is null)
                {
                    objectResult = new ObjectResult(mapper is null ? value : mapper(value!))
                    {
                        StatusCode = StatusCodes.Status201Created
                    };
                }
                else
                {
                    objectResult = new CreatedResult(location, mapper is null ? value : mapper(value!));
                }
            })
                                                                
#endif
            .OnSuccessNull(() => objectResult = null == nullCode ? new OkObjectResult(null) : nullCode())
            .OnFailed(errors => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<ActionResult<TResult>> ToActionCreatedResultAsync<TResult>(this Task<Result<TResult>> result, Uri? location, Func<StatusCodeResult>? nullCode = null)
    {
        return await ToActionCreatedResultAsync<TResult, TResult>(result, location, null, nullCode).ConfigureAwait(false);
    }

    public static async Task<ActionResult> ToActionOkResultAsync(this Task<Result> result)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult objectResult = new BadRequestResult();

        res.OnSuccess(() => objectResult = new NoContentResult())
           .OnFailed(_ => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static Task<ActionResult> ToActionOkResultAsync(this Result result)
    {
        ActionResult objectResult = new BadRequestResult();

        result.OnSuccess(() => objectResult = new NoContentResult())
              .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return Task.FromResult(objectResult);
    }

    // this Result<T>
    public static ValueTask<ActionResult<T>> ToActionOkResultAsync<TResult, T>(this Result<TResult> result, Func<TResult, T>? mapper, Func<StatusCodeResult>? nullCode = null)
    {
        ActionResult<T> objectResult = new BadRequestResult();

        result
            .OnSuccess((value) => objectResult = new OkObjectResult(mapper is null ? value : mapper(value)))
            .OnSuccessNull(() => objectResult = null == nullCode ? new OkObjectResult(null) : nullCode())
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return ValueTask.FromResult(objectResult);
    }

    public static ValueTask<ActionResult<TResult>> ToActionOkResultAsync<TResult>(this Result<TResult> result, Func<StatusCodeResult>? nullCode = null)
    {
        return ToActionOkResultAsync<TResult, TResult>(result, null, nullCode);
    }

    public static ValueTask<ActionResult<T>> ToActionCreatedResultAsync<TResult, T>(this Result<TResult> result, Uri? location, Func<TResult, T>? mapper = null, Func<StatusCodeResult>? nullCode = null)
    {
        ActionResult<T> objectResult = new BadRequestResult();

        result
#if NET8_0
            .OnSuccess(value => objectResult = new CreatedResult(location, mapper is null ? value : mapper(value!)))
#else
            .OnSuccess(value =>
            {
                if (location is null)
                {
                    objectResult = new ObjectResult(mapper is null ? value : mapper(value!))
                    {
                        StatusCode = StatusCodes.Status201Created
                    };
                }
                else
                {
                    objectResult = new CreatedResult(location, mapper is null ? value : mapper(value!));
                }
            })

#endif
            .OnSuccessNull(() => objectResult = null == nullCode ? new OkObjectResult(null) : nullCode())
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return ValueTask.FromResult(objectResult);
    }

    public static ValueTask<ActionResult<TResult>> ToActionCreatedResultAsync<TResult>(this Result<TResult> result, Uri? location, Func<StatusCodeResult>? nullCode = null)
    {
        return ToActionCreatedResultAsync<TResult, TResult>(result, location, null, nullCode);
    }

 
#endregion

#region TypedResult

#endregion
}
