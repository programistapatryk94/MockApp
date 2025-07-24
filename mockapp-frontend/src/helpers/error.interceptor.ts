import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpResponseBase,
} from '@angular/common/http';
import { Injectable, Injector } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { catchError, Observable, throwError } from 'rxjs';
import { SnackbarService } from '../app/shared/snackbar/snackbar.service';

export interface IErrorInfo {
  code: number;
  error: string;
}

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(
    private injector: Injector,
    private snackbarService: SnackbarService
  ) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error) => {
        return this.handleErrorResponse(error);
      })
    );
  }

  private handleErrorResponse(error: any): Observable<never> {
    const translate = this.injector.get(TranslateService);
    if (!(error.error instanceof Blob)) {
      if (error instanceof HttpErrorResponse) {
        if (error.status == 0) {
          return throwException(
            error.error,
            error.status,
            error.message,
            error.headers,
            null
          );
        } else {
          if (this.isErrorInfo(error?.error)) {
            var errorInfo = error.error;

            this.showError(errorInfo);

            return throwException(
              errorInfo.error,
              error.status,
              error.message,
              error.headers,
              null
            );
          }
          return throwError(() => error);
        }
      } else {
        return throwException(
          translate.instant('BLAD_SERWERA'),
          0,
          '',
          [],
          null
        );
      }
    }
    return throwError(() => error);
  }

  private isErrorInfo(error: any): error is IErrorInfo {
    return (
      error != null &&
      typeof error['__appError'] === 'boolean' &&
      error['__appError'] === true
    );
  }

  private showError(error: IErrorInfo) {
    return this.snackbarService.show({
      message: error.error,
      type: 'error',
    });
  }
}

export class ApiException extends Error {
  status: number;
  response: string;
  headers: { [key: string]: any };
  result: any;

  constructor(
    message: string,
    status: number,
    response: string,
    headers: { [key: string]: any },
    result: any
  ) {
    super();

    this.message = message;
    this.status = status;
    this.response = response;
    this.headers = headers;
    this.result = result;
  }

  protected isApiException = true;

  static isApiException(obj: any): obj is ApiException {
    return obj.isApiException === true;
  }
}

function throwException(
  message: string,
  status: number,
  response: string,
  headers: { [key: string]: any },
  result?: any
): Observable<never> {
  if (result !== null && result !== undefined) return throwError(() => result);
  else
    return throwError(
      () => new ApiException(message, status, response, headers, null)
    );
}
