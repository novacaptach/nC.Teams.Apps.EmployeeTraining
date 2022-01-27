// <copyright file="common-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import Constants from "../constants/constants";
import { getAPIRequestConfigParams } from "../helpers/api-helper";

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
