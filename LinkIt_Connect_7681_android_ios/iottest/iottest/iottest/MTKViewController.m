//
//  MTKViewController.m
//  iottest
//
//  Created by 杨源 on 14-5-25.
//  Copyright (c) 2014年 mediatek. All rights reserved.
//

#import "MTKViewController.h"
#import "SmartConnection.h"

@interface MTKViewController ()

@end

@implementation MTKViewController

- (void)viewDidLoad
{
    [super viewDidLoad];
	// Do any additional setup after loading the view, typically from a nib.
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (IBAction)OnStart:(UIButton *)sender {
    const char *ssid = [self.m_ssid.text cStringUsingEncoding:NSASCIIStringEncoding];
    const char *s_authmode = [self.m_authmode.text cStringUsingEncoding:NSASCIIStringEncoding];
    int authmode = atoi(s_authmode);
    const char *password = [self.m_password.text cStringUsingEncoding:NSASCIIStringEncoding];
    NSLog(@"OnStart: ssid = %s, authmode = %d, password = %s", ssid, authmode, password);
    InitSmartConnection();
    StartSmartConnection(ssid, password, "", authmode);
}

- (IBAction)OnStop:(UIButton *)sender {
    NSLog(@"OnStop");
}
@end
