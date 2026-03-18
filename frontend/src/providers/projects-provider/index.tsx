"use client";

import { ReactNode, useContext, useReducer } from "react";
import { getAxiosInstance } from "@/utils/axiosInstance";
import { ProjectReducer } from "./reducer";
import {
	INITIAL_STATE,
	IProjectCreateInput,
	IProjectItem,
	IProjectUpdateInput,
	ProjectActionContext,
	ProjectStateContext,
} from "./context";
import {
	createError,
	createPending,
	createSuccess,
	deleteError,
	deletePending,
	deleteSuccess,
	fetchAllError,
	fetchAllPending,
	fetchAllSuccess,
	fetchOneError,
	fetchOnePending,
	fetchOneSuccess,
	updateError,
	updatePending,
	updateSuccess,
} from "./actions";

const ENDPOINT = "/api/services/app/Project";

interface AbpResult<T> {
	result: T;
}

interface AbpPagedResult<T> {
	items: T[];
	totalCount: number;
}

export const ProjectProvider = ({ children }: { children: ReactNode }) => {
	const instance = getAxiosInstance();
	const [state, dispatch] = useReducer(ProjectReducer, INITIAL_STATE);

	const fetchAll = async () => {
		dispatch(fetchAllPending());
		try {
			const res = await instance.get<AbpResult<AbpPagedResult<IProjectItem>>>(`${ENDPOINT}/GetAll`);
			const { items, totalCount } = res.data.result;
			dispatch(fetchAllSuccess({ items, totalCount }));
		} catch {
			dispatch(fetchAllError());
		}
	};

	const fetchById = async (id: number) => {
		dispatch(fetchOnePending());
		try {
			const res = await instance.get<AbpResult<IProjectItem>>(`${ENDPOINT}/Get`, { params: { id } });
			dispatch(fetchOneSuccess(res.data.result));
		} catch {
			dispatch(fetchOneError());
		}
	};

	const create = async (data: IProjectCreateInput): Promise<IProjectItem> => {
		dispatch(createPending());
		try {
			const res = await instance.post<AbpResult<IProjectItem>>(`${ENDPOINT}/Create`, data);
			dispatch(createSuccess());
			dispatch(fetchOneSuccess(res.data.result));
			return res.data.result;
		} catch (error) {
			dispatch(createError());
			throw error;
		}
	};

	const update = async (data: IProjectUpdateInput) => {
		dispatch(updatePending());
		try {
			await instance.put(`${ENDPOINT}/Update`, data);
			dispatch(updateSuccess());
			await fetchAll();
		} catch {
			dispatch(updateError());
		}
	};

	const remove = async (id: number) => {
		dispatch(deletePending());
		try {
			await instance.delete(`${ENDPOINT}/Delete`, { params: { id } });
			dispatch(deleteSuccess());
			await fetchAll();
		} catch (error) {
			dispatch(deleteError());
			throw error;
		}
	};

	return (
		<ProjectStateContext.Provider value={state}>
			<ProjectActionContext.Provider value={{ fetchAll, fetchById, create, update, remove }}>
				{children}
			</ProjectActionContext.Provider>
		</ProjectStateContext.Provider>
	);
};

export const useProjectState = () => {
	const context = useContext(ProjectStateContext);
	if (!context) {
		throw new Error("useProjectState must be used within ProjectProvider");
	}
	return context;
};

export const useProjectAction = () => {
	const context = useContext(ProjectActionContext);
	if (!context) {
		throw new Error("useProjectAction must be used within ProjectProvider");
	}
	return context;
};

export {
	ProjectDatabaseOption,
	ProjectFramework,
	ProjectProgrammingLanguage,
	ProjectStatus,
} from "./context";
export type { IProjectCreateInput, IProjectItem, IProjectUpdateInput } from "./context";
