
export interface Project {
    id: string;
    name: string;
    apiPrefix: string;
    secret: string;
    createdAt: Date;
    membersEnabled: boolean;
}

export interface CreateProjectInput {
    name: string;
    apiPrefix: string;
}