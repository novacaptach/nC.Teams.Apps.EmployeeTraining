﻿// <copyright file="signin-end.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import React, { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";

const SignInSimpleEnd: React.FunctionComponent = () => {
  // Parse hash parameters into key-value pairs
  function getHashParameters() {
    const hashParams: any = {};
    window.location.hash
      .substr(1)
      .split("&")
      .forEach(function (item) {
        const s = item.split("=");
        const k = s[0];
        const v = s[1] && decodeURIComponent(s[1]);
        hashParams[k] = v;
      });
    return hashParams;
  }

  useEffect(() => {
    microsoftTeams.initialize();
    const hashParams: any = getHashParameters();
    if (hashParams["error"]) {
      // Authentication/authorization failed
      microsoftTeams.authentication.notifyFailure(hashParams["error"]);
    } else if (hashParams["id_token"]) {
      // Success
      microsoftTeams.authentication.notifySuccess();
      window.location.href = "/";
    } else {
      // Unexpected condition: hash does not contain error or access_token parameter
      microsoftTeams.authentication.notifyFailure("UnexpectedFailure");
    }
  });

  return <></>;
};

export default SignInSimpleEnd;
