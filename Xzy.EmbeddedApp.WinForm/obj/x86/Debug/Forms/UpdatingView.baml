   M S B A M L   `   `   `  ����  
qunkong3600    *Xzy.EmbeddedApp.WinForm.Forms.UpdatingView   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   <mah,http://metro.mahapps.com/winfx/xaml/controls     5   	   c _System.Windows.Interactivity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35Bi:http://schemas.microsoft.com/expression/2010/interactivity  5        � Title$  消息提示��5	   	   $	��150��6   $	��350��6#    � 
ResizeMode$ NoResize=�5
       � ShowInTaskbar .   � WindowStyle$
 None=�5       � WindowStartupLocation$ CenterScreen=�5       � Topmost . .  (System.Windows.Interactivity.Interaction   Triggers 5      /  )System.Windows.Interactivity.EventTrigger  5   
      	EventName$ Loaded��6   .  (System.Windows.Interactivity.TriggerBase   Actions. 5      6  0System.Windows.Interactivity.InvokeCommandAction  	   Command	 6$   �� *UpdatingCommand+6J   5      .��5      � .+�5   
   �� $��Center=�6   $��Center=�60   .+�5      �� -   5        
 ��Name$
 TbMsg��5      $��15(�6'   $��Center=�65   $��Center=�6P   $��15q�6m   $
��Wrap=�6y   ��6�   �� *	Message+6�   �� 5      -   5        $
 
TbProgress��5      $��20(�6,   $��Center=�6:   ��6W   �� *
Progress+6s   5      5      5      