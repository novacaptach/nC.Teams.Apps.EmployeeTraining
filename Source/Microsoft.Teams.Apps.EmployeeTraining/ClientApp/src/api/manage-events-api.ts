// <copyright file="manage-events-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import Constants from "../constants/constants";
import { getAPIRequestConfigParams } from "../helpers/api-helper";

/**
 * Gets LnD team events
 * @param searchString The events to load with matching search text
 * @param pageCount The page count of which events to be fetched
 * @param eventSearchType The events of particular status type that need to be retrieved
 * @param teamId The LnD team ID
 */
export const getEventsAsync = async (
  searchString: string,
  pageCount: number,
  eventSearchType: number,
  teamId: string
) => {
  const url = `${Constants.apiBaseURL}/EventWorkflow`;
  const config = getAPIRequestConfigParams({
    searchString: encodeURIComponent(searchString),
    eventSearchType: eventSearchType,
    teamId: teamId,
    pageCount: pageCount,
  });

  return await axios.get(url, config);
};

/**
 * Gets event details
 * @param eventId The event ID of which details need to be retrieved
 * @param teamId The LnD team ID
 */
export const getEventAsync = async (eventId: string, teamId: string) => {
  const url = `${Constants.apiBaseURL}/Event`;
  const config = getAPIRequestConfigParams({
    eventId: eventId,
    teamId: teamId,
  });

  return await axios.get(url, config);
};

/**
 * Closes the event registrations
 * @param teamId The LnD team ID
 * @param eventId The event ID of which registrations needs to be closed
 */
export const closeEventRegistrationsAsync = async (
  teamId: string,
  eventId: string
) => {
  const url = `${Constants.apiBaseURL}/EventWorkflow/CloseEventRegistrations`;
  const config = getAPIRequestConfigParams({
    eventId: eventId,
    teamId: teamId,
  });

  return await axios.patch(url, null, config);
};

/**
 * Cancels an event to occur
 * @param teamId The LnD team ID
 * @param eventId The event ID that needs to be cancelled
 */
export const cancelEventAsync = async (teamId: string, eventId: string) => {
  const url = `${Constants.apiBaseURL}/EventWorkflow/CancelEvent`;
  const config = getAPIRequestConfigParams({
    eventId: eventId,
    teamId: teamId,
  });

  return await axios.patch(url, null, config);
};

/**
 *
 * @param teamId The LnD team ID
 * @param eventId The draft event ID that needs to be deleted
 */
export const deleteDraftEventAsync = async (
  teamId: string,
  eventId: string
) => {
  const url = `${Constants.apiBaseURL}/EventWorkflow/delete-draft`;
  const config = getAPIRequestConfigParams({
    eventId: eventId,
    teamId: teamId,
  });

  return await axios.delete(url, config);
};

/**
 * Exports event details to CSV file
 * @param teamId The LnD team ID
 * @param eventId The event ID that of which details needs to be exported
 */
export const exportEventDetailsToCSV = async (
  teamId: string,
  eventId: string
) => {
  const url = `${Constants.apiBaseURL}/eventfiles/ExportEventDetailsToCSV`;
  const config = getAPIRequestConfigParams({
    eventId: eventId,
    teamId: teamId,
  });

  return await axios.get(url, config);
};

/**
 * Sends reminder to the users registered for the event
 * @param teamId The LnD team ID
 * @param eventId The event ID
 */
export const sendReminder = async (teamId: string, eventId: string) => {
  const url = `${Constants.apiBaseURL}/EventWorkflow/SendReminder`;
  const config = getAPIRequestConfigParams({
    eventId: eventId,
    teamId: teamId,
  });

  return await axios.post(url, null, config);
};
