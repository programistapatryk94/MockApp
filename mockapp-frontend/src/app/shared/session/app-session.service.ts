import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AppConfigService } from '../app-config.service';
import {
  Features,
  LocalizationConfigurationDto,
  UserInfoDto,
} from '../../../helpers/auth.model';
import { catchError, EMPTY, take } from 'rxjs';

@Injectable()
export class AppSessionService {
  private readonly apiUrl;
  private _userInfo!: UserInfoDto;

  constructor(private http: HttpClient, private config: AppConfigService) {
    this.apiUrl = `${this.config.remoteApiUrl}/api/auth`;
  }

  get localization(): LocalizationConfigurationDto {
    return this._userInfo.localization;
  }

  get features(): Features {
    return this._userInfo.features;
  }

  init() {
    return new Promise<boolean>((resolve, reject) => {
      this.http
        .get<UserInfoDto>(`${this.apiUrl}/me`)
        .pipe(
          take(1),
          catchError((err) => {
            reject(err);
            return EMPTY;
          })
        )
        .subscribe((result: UserInfoDto) => {
          this._userInfo = result;
          resolve(true);
        });
    });
  }
}
