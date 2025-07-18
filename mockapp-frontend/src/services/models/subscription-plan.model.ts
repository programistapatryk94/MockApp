
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