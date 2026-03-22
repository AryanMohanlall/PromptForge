"use client";

import { ReactNode, useCallback, useContext, useReducer } from "react";
import { getAxiosInstance } from "@/utils/axiosInstance";
import { TemplateReducer } from "./reducer";
import {
  INITIAL_STATE,
  ITemplateCreateInput,
  ITemplateItem,
  ITemplateUpdateInput,
  ITemplateListInput,
  TemplateActionContext,
  TemplateStateContext,
} from "./context";
import {
  clearItems,
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

const ENDPOINT = "/api/services/app/Template";

interface AbpResult<T> {
  result: T;
}

interface AbpPagedResult<T> {
  items: T[];
  totalCount: number;
}

export const TemplateProvider = ({ children }: { children: ReactNode }) => {
  const instance = getAxiosInstance();
  const [state, dispatch] = useReducer(TemplateReducer, INITIAL_STATE);

  const fetchAll = useCallback(
    async (input?: ITemplateListInput) => {
      // Clear current items before fetching new ones
      dispatch(clearItems());
      dispatch(fetchAllPending());
      try {
        const res = await instance.get<
          AbpResult<AbpPagedResult<ITemplateItem>>
        >(`${ENDPOINT}/GetList`, {
          params: input,
        });
        const { items, totalCount } = res.data.result;
        dispatch(fetchAllSuccess({ items, totalCount }));
      } catch {
        dispatch(fetchAllError());
      }
    },
    [instance],
  );

  const fetchById = useCallback(
    async (id: number) => {
      dispatch(fetchOnePending());
      try {
        const res = await instance.get<AbpResult<ITemplateItem>>(
          `${ENDPOINT}/Get`,
          {
            params: { id },
          },
        );
        dispatch(fetchOneSuccess(res.data.result));
      } catch {
        dispatch(fetchOneError());
      }
    },
    [instance],
  );

  const create = useCallback(
    async (data: ITemplateCreateInput) => {
      dispatch(createPending());
      try {
        await instance.post(`${ENDPOINT}/Create`, data);
        dispatch(createSuccess());
        await fetchAll();
      } catch (error) {
        dispatch(createError());
        throw error;
      }
    },
    [fetchAll, instance],
  );

  const update = useCallback(
    async (data: ITemplateUpdateInput) => {
      dispatch(updatePending());
      try {
        await instance.put(`${ENDPOINT}/Update`, data, {
          params: { id: data.id },
        });
        dispatch(updateSuccess());
        await fetchAll();
      } catch (error) {
        dispatch(updateError());
        throw error;
      }
    },
    [fetchAll, instance],
  );

  const remove = useCallback(
    async (id: number) => {
      dispatch(deletePending());
      try {
        await instance.delete(`${ENDPOINT}/Delete`, { params: { id } });
        dispatch(deleteSuccess());
        await fetchAll();
      } catch (error) {
        dispatch(deleteError());
        throw error;
      }
    },
    [fetchAll, instance],
  );

  const publish = useCallback(
    async (id: number) => {
      try {
        await instance.post(`${ENDPOINT}/Publish`, null, { params: { id } });
        await fetchAll();
      } catch (error) {
        throw error;
      }
    },
    [fetchAll, instance],
  );

  const deprecate = useCallback(
    async (id: number) => {
      try {
        await instance.post(`${ENDPOINT}/Deprecate`, null, { params: { id } });
        await fetchAll();
      } catch (error) {
        throw error;
      }
    },
    [fetchAll, instance],
  );

  const setFeatured = useCallback(
    async (id: number, featured: boolean) => {
      try {
        await instance.post(`${ENDPOINT}/SetFeatured`, null, {
          params: { id, featured },
        });
        await fetchAll();
      } catch (error) {
        throw error;
      }
    },
    [fetchAll, instance],
  );

  const toggleFavorite = useCallback(
    async (id: number) => {
      try {
        await instance.post(`${ENDPOINT}/ToggleFavorite`, null, {
          params: { id },
        });
        // We can either fetchAll or just update the local state for better UX
        // For simplicity, let's just fetchAll or the single item
        await fetchAll();
      } catch (error) {
        throw error;
      }
    },
    [fetchAll, instance],
  );

  return (
    <TemplateStateContext.Provider value={state}>
      <TemplateActionContext.Provider
        value={{
          fetchAll,
          fetchById,
          create,
          update,
          remove,
          publish,
          deprecate,
          setFeatured,
          toggleFavorite,
        }}
      >
        {children}
      </TemplateActionContext.Provider>
    </TemplateStateContext.Provider>
  );
};

export const useTemplateState = () => {
  const context = useContext(TemplateStateContext);
  if (!context) {
    throw new Error("useTemplateState must be used within TemplateProvider");
  }
  return context;
};

export const useTemplateAction = () => {
  const context = useContext(TemplateActionContext);
  if (!context) {
    throw new Error("useTemplateAction must be used within TemplateProvider");
  }
  return context;
};

export type {
  ITemplateCreateInput,
  ITemplateItem,
  ITemplateUpdateInput,
} from "./context";
