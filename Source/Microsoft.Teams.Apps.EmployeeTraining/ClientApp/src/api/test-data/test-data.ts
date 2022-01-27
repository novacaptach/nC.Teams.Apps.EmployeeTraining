import { IConstantDropdownItem } from "../../constants/resources";
import { EventAudience } from "../../models/event-audience";
import { EventType } from "../../models/event-type";
import { IEvent } from "../../models/IEvent";
import { ISelectedUserGroup } from "../../models/ISelectedUserGroup";
import { ICreateEventState } from "../../components/create-event/create-event-wrapper";

export default class TestData {
  static dummyText = (length: number) => {
    let text = "random ";
    for (let i = 0; i < length / 5; i += 1) {
      text = text.concat(text);
    }
    return text;
  };

  static stateTest: ICreateEventState = {
    currentEventStep: 1,
    categories: new Array<IConstantDropdownItem>(),
    displayReadonly: false,
    eventDetails: {
      categoryId: "",
      categoryName: "",
      createdBy: "",
      createdOn: new Date(),
      description: "",
      endDate: new Date(new Date().getDate() + 1),
      eventId: "",
      graphEventId: "",
      isAutoRegister: false,
      isRegistrationClosed: false,
      maximumNumberOfParticipants: 0,
      meetingLink: "",
      name: "",
      numberOfOccurrences: 1,
      photo: "",
      registeredAttendeesCount: 0,
      startDate: new Date(new Date().getDate() + 1),
      status: 0,
      teamId: "",
      type: EventType.Teams,
      venue: "",
      audience: EventAudience.Public,
      endTime: moment.utc(new Date()).local().toDate(),
      startTime: moment.utc(new Date()).local().toDate(),
      mandatoryAttendees: "",
      optionalAttendees: "",
      registeredAttendees: "",
      selectedUserOrGroupListJSON: "",
      autoRegisteredAttendees: "",
    },
    selectedCategory: undefined,
    selectedEvent: undefined,
    selectedAudience: undefined,
    selectedUserGroups: new Array<ISelectedUserGroup>(),
    isEdit: false,
    isDraft: false,
    isLoading: true,
  };

  static draftStateTest: ICreateEventState = {
    currentEventStep: 1,
    categories: new Array<IConstantDropdownItem>(),
    displayReadonly: false,
    eventDetails: {
      categoryId: "",
      categoryName: "",
      createdBy: "",
      createdOn: new Date(),
      description: "",
      endDate: new Date(new Date().getDate() + 1),
      eventId: "",
      graphEventId: "",
      isAutoRegister: false,
      isRegistrationClosed: false,
      maximumNumberOfParticipants: 0,
      meetingLink: "",
      name: "",
      numberOfOccurrences: 1,
      photo: "",
      registeredAttendeesCount: 0,
      startDate: new Date(new Date().getDate() + 1),
      status: 0,
      teamId: "",
      type: EventType.Teams,
      venue: "",
      audience: EventAudience.Public,
      endTime: moment.utc(new Date()).local().toDate(),
      startTime: moment.utc(new Date()).local().toDate(),
      mandatoryAttendees: "",
      optionalAttendees: "",
      registeredAttendees: "",
      selectedUserOrGroupListJSON: "",
      autoRegisteredAttendees: "",
    },
    selectedCategory: undefined,
    selectedEvent: undefined,
    selectedAudience: undefined,
    selectedUserGroups: new Array<ISelectedUserGroup>(),
    isEdit: true,
    isDraft: false,
    isLoading: true,
  };

  static testEventDetails: IEvent = {
    categoryId: "1",
    categoryName: "Entertainment",
    createdBy: "some",
    createdOn: new Date(),
    description: "Event Description",
    endDate: new Date(new Date().getDate() + 1),
    eventId: "1",
    graphEventId: "abc",
    isAutoRegister: false,
    isRegistrationClosed: false,
    maximumNumberOfParticipants: 10,
    meetingLink: "",
    name: "Test Event",
    numberOfOccurrences: 1,
    photo: "",
    registeredAttendeesCount: 0,
    startDate: new Date(new Date().getDate() + 1),
    status: 0,
    teamId: "",
    type: EventType.Teams,
    venue: "",
    audience: EventAudience.Public,
    endTime: moment.utc(new Date()).local().toDate(),
    startTime: moment.utc(new Date()).local().toDate(),
    mandatoryAttendees: "",
    optionalAttendees: "",
    registeredAttendees: "",
    selectedUserOrGroupListJSON: "",
    autoRegisteredAttendees: "",
  };
}
