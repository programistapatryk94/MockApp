
export interface ISubscriptionPlanDto {
    id: string;
    maxProjects: number;
    maxResources: number;
    hasCollaboration: boolean;
    prices: ISubscriptionPlanPriceDto[];
}

export interface ISubscriptionPlanPriceDto {
    id: string;
    name: string;
    currency: string;
    amount: number;
}

export interface ICurrentSubscriptionInfo {
    id: string;
    maxProjects: number;
    maxResources: number;
    hasCollaboration: boolean;
    isCanceling: boolean;
    currentPeriodEnd: Date;
    prices: ISubscriptionPlanPriceDto[];
}