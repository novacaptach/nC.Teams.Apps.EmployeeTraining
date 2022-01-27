// <copyright file="create-event-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import { IEvent } from "../models/IEvent";
import Constants from "../constants/constants";
import { getAPIRequestConfigParams } from "../helpers/api-helper";

/**
 * Upload image photo
 * @param formData Form data containing selected image
 * @param teamId The LnD team Id
 */
export const uploadEventImage = async (formData: FormData, teamId: string) => {
  const url = `${Constants.apiBaseURL}/eventfiles/upload-image`;
  const config = getAPIRequestConfigParams({ teamId: teamId });

  return await axios.post(url, formData, config);
};

/**
 * Save event as draft
 * @param event Event details to be saved as draft
 * @param teamId The LnD team Id
 */
export const saveEventAsDraft = async (event: IEvent, teamId: string) => {
  const url = `${Constants.apiBaseURL}/eventworkflow/create-draft`;
  const config = getAPIRequestConfigParams({ teamId: teamId });

  return await axios.post(url, event, config);
};

/**
 * Update draft event
 * @param event Event details to be updated as draft
 * @param teamId The LnD team Id
 */
export const updateEventAsDraft = async (event: IEvent, teamId: string) => {
  const url = `${Constants.apiBaseURL}/eventworkflow/update-draft`;
  const config = getAPIRequestConfigParams({ teamId: teamId });

  return await axios.patch(url, event, config);
};

/**
 * Create event and add to calendar
 * @param event Event details to be saved
 * @param teamId The LnD team Id
 */
export const createNewEvent = async (event: IEvent, teamId: string) => {
  const url = `${Constants.apiBaseURL}/eventworkflow/create-event`;
  const config = getAPIRequestConfigParams({ teamId: teamId });

  return await axios.post(url, event, config);
};

/**
 * Update event details
 * @param event Event details to be updated
 * @param teamId The LnD team Id
 */
export const updateEvent = async (event: IEvent, teamId: string) => {
  const url = `${Constants.apiBaseURL}/eventworkflow/update-event`;
  const config = getAPIRequestConfigParams({ teamId: teamId });

  return await axios.patch(url, event, config);
};

/**
 * Gets event categories
 * @param teamId The LnD team Id
 */
export const getEventCategoriesAsync = async () => {
  const url = `${Constants.apiBaseURL}/category/get-categories-for-event`;
  return await axios.get(url);
};

/**
 * Check for event with same name
 * @param eventName User entered event name
 */
export const searchEventAsync = async (eventName: string) => {
  const url = `${Constants.apiBaseURL}/event/search-by-title`;
  const config = getAPIRequestConfigParams({ search: eventName });

  return await axios.get(url, config);
};
