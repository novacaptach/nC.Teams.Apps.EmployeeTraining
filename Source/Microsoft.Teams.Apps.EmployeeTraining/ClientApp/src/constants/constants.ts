/*
    <copyright file="constants.ts" company="Microsoft">
    Copyright (c) Microsoft. All rights reserved.
    </copyright>
*/

export default class Constants {
  //Themes
  static readonly body = "body";
  static readonly theme = "theme";
  static readonly default = "default";
  static readonly light = "light";
  static readonly dark = "dark";
  static readonly contrast = "contrast";

  //Constants for manage categories
  static readonly categoryNameMaxLength = 100;
  static readonly categoryDescriptionMaxLength = 300;

  static readonly lazyLoadEventsCount = 50;

  static readonly maxWidthForMobileView = 750;

  /** The base URL for API */
  static readonly apiBaseURL = window.location.origin + "/api";
}

/** Indicates the operations that can be done on event categories */
export enum CategoryOperations {
  Add,
  Edit,
  Delete,
  Unknown,
}

/** Indicates the response status codes */
export enum ResponseStatus {
  OK = 200,
}
