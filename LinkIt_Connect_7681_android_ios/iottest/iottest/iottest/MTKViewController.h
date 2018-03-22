//
//  MTKViewController.h
//  iottest
//
//  Created by 杨源 on 14-5-25.
//  Copyright (c) 2014年 mediatek. All rights reserved.
//

#import <UIKit/UIKit.h>

@interface MTKViewController : UIViewController
@property (weak, nonatomic) IBOutlet UITextField *m_ssid;
@property (weak, nonatomic) IBOutlet UITextField *m_authmode;
@property (weak, nonatomic) IBOutlet UITextField *m_password;
- (IBAction)OnStart:(UIButton *)sender;
- (IBAction)OnStop:(UIButton *)sender;

@end
