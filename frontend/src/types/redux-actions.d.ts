declare module "redux-actions" {
  export type Action<Payload = unknown> = {
    type: string;
    payload?: Payload;
  };

  export type Reducer<State> = (state: State | undefined, action: Action) => State;

  export type ActionFunction<
    Payload = unknown,
    Args extends unknown[] = unknown[]
  > = (...args: Args) => Action<Payload>;

  export function createAction<
    Payload = unknown,
    Args extends unknown[] = unknown[]
  >(
    type: string,
    payloadCreator?: (...args: Args) => Payload
  ): ActionFunction<Payload, Args>;

  export function handleActions<State, Payload = unknown>(
    handlers: Record<string, (state: State, action: { payload: Payload }) => State>,
    defaultState: State
  ): Reducer<State>;
}
