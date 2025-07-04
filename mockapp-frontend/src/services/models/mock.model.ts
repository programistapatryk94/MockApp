
export interface Mock {
    id: string;
    userId: string;
    urlPath: string;
    method: string;
    statusCode: number;
    responseBody: string;
    headersJson?: string;
    createdAt: Date;
}

export interface CreateMockInput {
    urlPath: string;
    method: string;
    statusCode: number;
    responseBody: string;
    headersJson?: string;
}