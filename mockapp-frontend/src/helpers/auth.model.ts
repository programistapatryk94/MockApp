export interface CreateUserInput {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
}

export interface CurrentCultureConfigDto {
  name: string;
  displayName: string;
}

export interface LanguageInfo {
  name: string;
  displayName: string;
  isDefault: boolean;
  icon: string;
}

export interface LocalizationConfigurationDto {
  currentCulture: CurrentCultureConfigDto;
  languages: LanguageInfo[];
}

export interface Features {
  collaborationEnabled: boolean;
  defaultLanguage: string;
  maxMockCreationLimit: number;
  maxProjectCreationLimit: number;
}

export interface UserInfoDto {
  localization: LocalizationConfigurationDto;
  features: Features;
}
