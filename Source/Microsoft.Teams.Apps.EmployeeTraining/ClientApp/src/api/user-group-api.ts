// <copyright file="user-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import Constants from "../constants/constants";
import { getAPIRequestConfigParams } from "../helpers/api-helper";

/**
 * Search users and groups.
 * @param searchText defines the searched text
 */
export const searchUsersAndGroups = async (
  searchText: string
): Promise<any> => {
  const url = `${Constants.apiBaseURL}/users`;
  const config = getAPIRequestConfigParams({
    searchText: encodeURIComponent(searchText),
  });

  return await axios.get(url, config);
};

/**
 * Get members of group.
 * @param groupId identifies a particular group of members
 */
export const getGroupMembers = async (groupId: string): Promise<any> => {
  const url = `${Constants.apiBaseURL}/group/get-group-members`;
  const config = getAPIRequestConfigParams({ groupId: groupId });

  return await axios.get(url, config);
};

/**
 * Gets the user profiles
 * @param userIds The user IDs of which profiles to get
 */
export const getUserProfiles = async (userIds: Array<string>): Promise<any> => {
  const url = `${Constants.apiBaseURL}/users`;
  return await axios.post(url, userIds);
};
