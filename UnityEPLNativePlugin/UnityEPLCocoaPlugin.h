//
//  UnityEPLCocoaPlugin.h
//  UnityEPLNativePlugin
//
//  Created by Henry Jonas Solberg on 7/31/17.
//

#import <Foundation/Foundation.h>
#import <Cocoa/Cocoa.h>

NSMutableArray * KeyCodeQueue;
NSMutableArray * KeyTimestampQueue;
NSMutableArray * MouseButtonQueue;
NSMutableArray * MouseTimestampQueue;

double StartCocoaPlugin(void);

int PopKeyKeycode(void);
float PopKeyTimestamp(void);
int CountKeyEvents(void);
int PopMouseButton(void);
float PopMouseTimestamp(void);
int CountMouseEvents(void);
void handleInputEvent(NSEvent * theEvent);
