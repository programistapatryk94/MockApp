import { Injectable } from '@angular/core';
import { AppConsts } from '../shared/AppConsts';

@Injectable({ providedIn: 'root' })
export class AppConfigService {
  get remoteApiUrl(): string {
    return AppConsts.remoteServiceBaseUrl;
  }
}
