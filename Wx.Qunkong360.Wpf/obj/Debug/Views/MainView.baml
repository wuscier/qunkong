   M S B A M L   `   `   `  ����E  Aqunkong360, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:Wx.Qunkong360.WpfWx.Qunkong360.Wpf  5   	   [ WSystem.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Y@clr-namespace:System.Windows.Forms;assembly=System.Windows.FormsSystem.Windows.Forms 5	           Wx.Qunkong360.Wpf.MainView   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   1localclr-namespace:Wx.Qunkong360.Wpf     5   	   H DMahApps.Metro, Version=1.6.5.1, Culture=neutral, PublicKeyToken=null8metro,http://metro.mahapps.com/winfx/xaml/controls  5   	   V RMaterialDesignThemes.Wpf, Version=2.4.0.1044, Culture=neutral, PublicKeyToken=null:md1http://materialdesigninxaml.net/winfx/xaml/themes  5   	   Nwinform@clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms  5	      -   5          � Title$  MainView��5   	   $	��768��6   $
��1366��6'   $
��1366��64   $	��768��6D    � WindowStartupLocation$ CenterScreen=�5      $��16(�6W   ���#0956EE J� InputBindings 5      �� 5   
   
 �� Key$ T��6    �� 	Modifiers$	 Altf�6    �� Command 6-   �� *OpenTestViewCommand+6W   .��5      �  � 	Resources 5   
    �� 
TargetType   MaterialDesignRaisedButton �� BasedOn	 �� Property
 �� Value��%-  '��       7   '�� �     �� 5      " ��6   8   .�5      �� 	 � ��

 �����6?   �� 5      	 � ��
 �#2196F36A   �� 5      	 � ��$
 16(�6-   6:   5      �� 5      " ��6   .�5      �� 	 � ��

 �����6?   �� 5      	 � ��$
 14(�6-   6:   5       5!       � RowDefinitions 5#   
   �� 5$      $
��auto �6   6,   �� 5%      6    � ColumnDefinitions 5(   
   �� 5)      $	��250 �6   6-   �� 5*      6!   .+�5-   
   ��  �� Orientation$ 
Horizontal=�6   $��1��6.   $
��Left=�6>   .+�56      �� -   5         ��Name$ btnLaunchWhatsApp��56      $��35��60   $��10q�6<   6j   �� 57      -   5        $ btnCloseWhatsApp��57      $��35��60   $��10q�6<   6i   �� 59      -   5        $ btnLaunchMessenger��59      $��35��61   $��10q�6=   6l   �� 5:      -   5        $ btnCloseMessenger��5:      $��35��61   $��10q�6=   6k   �� 5<      -   5        $ btnCloseAll��5<      $��35��6*   $��10q�66   6^   �� 5>      -   5        $ btnOperation��5>      $��35��6+   $��10q�67   6`   �� 5B      -   5        $ btnInstallApk��5B      $��35��6,   $��10q�68   6b   �� 5D      -	   5        $ 
btnMonitor��5D      $��35��6)   $��10q�65   6\   5E      7  1System.Windows.Forms.Integration.WindowsFormsHost  5G   
   $��1��6   $��1��6(   $��2��68      Child. 5H         System.Windows.Forms.Panel  -
   5        	container��,  &System.Windows.Forms.ScrollableControl   
AutoScroll . "  System.Windows.Forms.Control   	BackColorU	 QSystem.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a# 	 System.Drawing.ColorConverter$ Transparent 5H   A   6[   5I      � 5K   
   $��1��6   $��8q�6    5M      �� 5N      6"   �� 5O      $��80 �6    6.   �� 5P      $
��auto �6    60   .+�5S      �� -   5         ��Name$ 
listGroups��5S      5[      � 5]      $��1��6    5^      �� 5_      $��2* �6'   64   �� 5`      6)   .+�5c      �� -   5         ��Name$ lblCurrentTaskItem��5c      $��Right=�68   $��20(�6T   $��Center=�6b   
�������$��0 0 5 0q�6�   当前任务：6�   �� 5d      -   5        $ lblCurrentTaskValue��5d      $��1��69   $
��Left=�6I   $��20(�6d   $��Center=�6r   
�������$��5 0 0 0q�6�   0/06�   5e      �� 5g      $��2��6   .+�5h      �� -   5        $ tbCopyright1��5h      $��8q�62   $
��Wrap=�6=   6S   �� 5i      -   5        $ tbCopyright2��5i      $��8q�62   $
��Wrap=�6=   6S   5j      5l      5n      5o      