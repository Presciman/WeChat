   M S B A M L   `   `   `  ����E  AWeChatDemo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:WeChatDemo
WeChatDemo  5   	       WeChatDemo.QrCodeDisplay   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:WeChatDemo     5   	     � 
ResizeMode$  NoResize=�5       � WindowStartupLocation$ CenterOwner=�60    � Title$ 微信扫码支付��5   	   $	��400��6   $	��250��6%   .��5	      � 
������� � RowDefinitions 5
   
   �� 5      $
��auto �6   �� 5      $��* �6   �� 5      $
��auto �6   .+�5   
   �� $��Center=�6   $��Center=�6-   $��Assets/WePayLogo.png��6H   $��
10,10,10,0q�6f   �� 5   
   $��Uniform=�6   $��10q�6"   $��Center=�6.   $��Center=�6K   $��1��6f   ��5      H� 5       H� Drawing 5      � 5      -   5        	qrDrawing�� � Brush
 �   �5      5      5      �� 5   
   $��Center=�6   $��Center=�6-   $��Assets/说明文字.png��6H   $��2��6a   $��
10,0,10,10q�6n   5      5      