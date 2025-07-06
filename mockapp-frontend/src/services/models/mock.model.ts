
export interface Mock {
    id: string;
    userId: string;
    urlPath: string;
    method: string;
    statusCode: number;
    responseBody: string;
    headersJson?: string;
    createdAt: Date;
    enabled: boolean;
    projectId: string;
}

export interface CreateMockInput {
    urlPath: string;
    method: string;
    statusCode: number;
    responseBody: string;
    headersJson?: string;
    projectId: string;
    enabled: boolean;
}