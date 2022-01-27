// <copyright file="api-helper.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import baseAxios from "axios";

/**
 * Gets the API request configuration parameters
 * @param params The request parameters
 */
export const getAPIRequestConfigParams = (params: any) => {
  const config = baseAxios.defaults;
  config.params = params;

  return config;
};
