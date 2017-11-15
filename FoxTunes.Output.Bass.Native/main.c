#include "main.h"

DWORD channelMaster = 0;
DWORD channelPrimary = 0;
DWORD channelSecondary = 0;

DWORD CALLBACK GaplessProc(HSTREAM handle, void *buffer, DWORD length, void *user) {
	DWORD result;
	if (channelPrimary && BASS_ChannelIsActive(channelPrimary)) {
		result = BASS_ChannelGetData(channelPrimary, buffer, length);
	}
	else if (channelSecondary && BASS_ChannelIsActive(channelSecondary)) {
		result = BASS_ChannelGetData(channelSecondary, buffer, length);
		channelPrimary = channelSecondary;
		channelSecondary = 0;
	}
	else {
		result = BASS_STREAMPROC_END;
	}
	return result;
}

HSTREAM BASSDEF(BASS_StreamCreateGaplessMaster)(DWORD freq, DWORD chans, DWORD flags, void *user) {
	return channelMaster = BASS_StreamCreate(freq, chans, flags, &GaplessProc, user);
}

DWORD BASSDEF(BASS_ChannelGetGaplessPrimary)() {
	return channelPrimary;
}

BOOL BASSDEF(BASS_ChannelSetGaplessPrimary)(DWORD channel) {
	channelPrimary = channel;
	return BASS_OK;
}

DWORD BASSDEF(BASS_ChannelGetGaplessSecondary)() {
	return channelSecondary;
}

BOOL BASSDEF(BASS_ChannelSetGaplessSecondary)(DWORD channel) {
	channelSecondary = channel;
	return BASS_OK;
}