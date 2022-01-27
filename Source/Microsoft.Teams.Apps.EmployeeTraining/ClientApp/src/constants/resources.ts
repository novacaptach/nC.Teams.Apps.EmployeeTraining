/*
	<copyright file="resources.ts" company="Microsoft">
	Copyright (c) Microsoft. All rights reserved.
	</copyright>
*/

import { EventAudience } from "../models/event-audience";
import { EventType } from "../models/event-type";
import { IPostType } from "../models/IPostType";
import { SortBy } from "../models/sort-by";

export interface IConstantDropdownItem {
  name: string;
  id: number;
}

export interface ITimeZonesItem {
  displayName: string;
  id: string;
}

export default class Resources {
  static readonly dark = "dark";
  static readonly contrast = "contrast";
  static readonly eventNameMaxLength = 100;
  static readonly eventDescriptionMaxLength = 1000;
  static readonly eventVenueMaxLength = 200;
  static readonly userEventsMobileFilteredCategoriesLocalStorageKey =
    "user-events-filtered-categories";
  static readonly userEventsMobileFilteredUsersLocalStorageKey =
    "user-events-filtered-users";
  static readonly userEventsMobileSortByFilterLocalStorageKey =
    "user-events-sortby";
  static readonly validUrlRegExp = /^http(s)?:\/\/(www\.)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$/;

  /** Color codes used while creating an event */
  static readonly colorCells = [
    { id: "a", label: "Wild blue yonder", color: "#A4A8CB" },
    { id: "b", label: "Jasmine", color: "#FFDE85" },
    { id: "c", label: "Sky blue", color: "#A0EAF8" },
    { id: "d", label: "Nadeshiko pink", color: "#F1A7B9" },
    { id: "e", label: "Lavender blue", color: "#E3D7FF" },
  ];

  /** Color codes used while creating an event */
  static readonly audienceType: Array<IConstantDropdownItem> = [
    { name: "Public", id: EventAudience.Public },
    { name: "Private", id: EventAudience.Private },
  ];

  /** Sort by values for filter */
  static readonly sortBy: Array<IPostType> = [
    { name: "Newest", id: SortBy.Recent, color: "" },
    { name: "Popularity", id: SortBy.Popularity, color: "" },
  ];

  /** Event type values */
  static readonly eventType: Array<IConstantDropdownItem> = [
    { name: "In person", id: EventType.InPerson },
    { name: "Teams", id: EventType.Teams },
    { name: "Live event", id: EventType.LiveEvent },
  ];
}
