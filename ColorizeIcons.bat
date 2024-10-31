
REM : Setup paths
set color_exe=IconColorizer\bin\Release\IconColorizer.exe
set source_path=SprueKit\Images
set out_path=SprueKit\Images

REM : Parameter value declarations, shared amonst all usages
set MAX_VALUE=224
set WHITE=255,255,255
set RED=255,80,80
set GREEN=80,255,80
set BLUE=27,161,226
set YELLOW=255,255,40
set ORANGE=255,150,0

start %color_exe %source_path%\godot\icon_omni_light.png       %out_path%\icon_light.png       force=%MAX_VALUE color=%WHITE

start %color_exe %source_path%\godot\icon_box_shape.png       %out_path%\icon_box_shape_white.png       max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\godot\icon_box_shape.png       %out_path%\icon_box_shape_red.png         max=%MAX_VALUE color=%RED
start %color_exe %source_path%\godot\icon_box_shape.png       %out_path%\icon_box_shape_green.png       max=%MAX_VALUE color=%GREEN
start %color_exe %source_path%\godot\icon_box_shape.png       %out_path%\icon_box_shape_blue.png        max=%MAX_VALUE color=%BLUE
start %color_exe %source_path%\godot\icon_box_shape.png       %out_path%\icon_box_shape_orange.png      max=%MAX_VALUE color=%ORANGE

start %color_exe %source_path%\godot\icon_plane.png       %out_path%\icon_plane_white.png       max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\godot\icon_plane.png       %out_path%\icon_plane_red.png         max=%MAX_VALUE color=%RED
start %color_exe %source_path%\godot\icon_plane.png       %out_path%\icon_plane_green.png       max=%MAX_VALUE color=%GREEN
start %color_exe %source_path%\godot\icon_plane.png       %out_path%\icon_plane_blue.png        max=%MAX_VALUE color=%BLUE
start %color_exe %source_path%\godot\icon_plane.png       %out_path%\icon_plane_orange.png      max=%MAX_VALUE color=%ORANGE

start %color_exe %source_path%\godot\icon_mesh_instance.png       %out_path%\icon_mesh_white.png       max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\godot\icon_mesh_instance.png       %out_path%\icon_mesh_red.png         max=%MAX_VALUE color=%RED
start %color_exe %source_path%\godot\icon_mesh_instance.png       %out_path%\icon_mesh_green.png       max=%MAX_VALUE color=%GREEN
start %color_exe %source_path%\godot\icon_mesh_instance.png       %out_path%\icon_mesh_blue.png        max=%MAX_VALUE color=%BLUE
start %color_exe %source_path%\godot\icon_mesh_instance.png       %out_path%\icon_mesh_orange.png      max=%MAX_VALUE color=%ORANGE

start %color_exe %source_path%\godot\icon_path.png       %out_path%\icon_path_white.png       force=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\godot\icon_path.png       %out_path%\icon_path_red.png         force=%MAX_VALUE color=%RED
start %color_exe %source_path%\godot\icon_path.png       %out_path%\icon_path_green.png       force=%MAX_VALUE color=%GREEN
start %color_exe %source_path%\godot\icon_path.png       %out_path%\icon_path_blue.png        force=%MAX_VALUE color=%BLUE
start %color_exe %source_path%\godot\icon_path.png       %out_path%\icon_path_orange.png      force=%MAX_VALUE color=%ORANGE

start %color_exe %source_path%\godot\icon_bone.png       %out_path%\icon_bone_white.png       max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\godot\icon_bone.png       %out_path%\icon_bone_red.png         max=%MAX_VALUE color=%RED
start %color_exe %source_path%\godot\icon_bone.png       %out_path%\icon_bone_green.png       max=%MAX_VALUE color=%GREEN
start %color_exe %source_path%\godot\icon_bone.png       %out_path%\icon_bone_blue.png        max=%MAX_VALUE color=%BLUE
start %color_exe %source_path%\godot\icon_bone.png       %out_path%\icon_bone_orange.png      max=%MAX_VALUE color=%ORANGE

start %color_exe %source_path%\godot\icon_add.png       %out_path%\icon_add_white.png       max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\godot\icon_add.png       %out_path%\icon_add_red.png         max=%MAX_VALUE color=%RED
start %color_exe %source_path%\godot\icon_add.png       %out_path%\icon_add_green.png       max=%MAX_VALUE color=%GREEN
start %color_exe %source_path%\godot\icon_add.png       %out_path%\icon_add_blue.png        max=%MAX_VALUE color=%BLUE
start %color_exe %source_path%\godot\icon_add.png       %out_path%\icon_add_orange.png      max=%MAX_VALUE color=%ORANGE

start %color_exe %source_path%\godot\icon_edit.png       %out_path%\icon_edit_white.png       max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\godot\icon_edit.png       %out_path%\icon_edit_red.png         max=%MAX_VALUE color=%RED
start %color_exe %source_path%\godot\icon_edit.png       %out_path%\icon_edit_green.png       max=%MAX_VALUE color=%GREEN
start %color_exe %source_path%\godot\icon_edit.png       %out_path%\icon_edit_blue.png        max=%MAX_VALUE color=%BLUE
start %color_exe %source_path%\godot\icon_edit.png       %out_path%\icon_edit_orange.png      max=%MAX_VALUE color=%ORANGE
start %color_exe %source_path%\godot\icon_edit.png       %out_path%\icon_edit_yellow.png      max=%MAX_VALUE color=%YELLOW

start %color_exe %source_path%\godot\icon_remove.png       %out_path%\icon_remove_white.png       max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\godot\icon_remove.png       %out_path%\icon_remove_red.png         max=%MAX_VALUE color=%RED
start %color_exe %source_path%\godot\icon_remove.png       %out_path%\icon_remove_green.png       max=%MAX_VALUE color=%GREEN
start %color_exe %source_path%\godot\icon_remove.png       %out_path%\icon_remove_blue.png        max=%MAX_VALUE color=%BLUE
start %color_exe %source_path%\godot\icon_remove.png       %out_path%\icon_remove_orange.png      max=%MAX_VALUE color=%ORANGE

start %color_exe %source_path%\godot\icon_arrow_right.png       %out_path%\icon_arrow_right_white.png       max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\godot\icon_arrow_right.png       %out_path%\icon_arrow_right_red.png         max=%MAX_VALUE color=%RED
start %color_exe %source_path%\godot\icon_arrow_right.png       %out_path%\icon_arrow_right_green.png       max=%MAX_VALUE color=%GREEN
start %color_exe %source_path%\godot\icon_arrow_right.png       %out_path%\icon_arrow_right_blue.png        max=%MAX_VALUE color=%BLUE
start %color_exe %source_path%\godot\icon_arrow_right.png       %out_path%\icon_arrow_right_orange.png      max=%MAX_VALUE color=%ORANGE
start %color_exe %source_path%\godot\icon_arrow_right.png       %out_path%\icon_arrow_right_yellow.png      max=%MAX_VALUE color=%YELLOW

start %color_exe %source_path%\godot\icon_blend.png       %out_path%\icon_blend_white.png       max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\godot\icon_blend.png       %out_path%\icon_blend_red.png         max=%MAX_VALUE color=%RED
start %color_exe %source_path%\godot\icon_blend.png       %out_path%\icon_blend_green.png       max=%MAX_VALUE color=%GREEN
start %color_exe %source_path%\godot\icon_blend.png       %out_path%\icon_blend_blue.png        max=%MAX_VALUE color=%BLUE
start %color_exe %source_path%\godot\icon_blend.png       %out_path%\icon_blend_orange.png      max=%MAX_VALUE color=%ORANGE
start %color_exe %source_path%\godot\icon_blend.png       %out_path%\icon_blend_yellow.png      max=%MAX_VALUE color=%YELLOW

start %color_exe %source_path%\godot\icon_world_environment.png       %out_path%\icon_world_white.png  force=%MAX_VALUE color=%WHITE

start %color_exe %source_path%\godot\icon_position_3d.png       %out_path%\icon_point_white.png  force=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\godot\icon_position_3d.png       %out_path%\icon_point_green.png  force=%MAX_VALUE color=%GREEN
start %color_exe %source_path%\godot\icon_position_3d.png       %out_path%\icon_point_yellow.png  force=%MAX_VALUE color=%YELLOW
start %color_exe %source_path%\godot\icon_position_3d.png       %out_path%\icon_point_orange.png  force=%MAX_VALUE color=%ORANGE
start %color_exe %source_path%\godot\icon_position_3d.png       %out_path%\icon_point_red.png  force=%MAX_VALUE color=%RED
start %color_exe %source_path%\godot\icon_position_3d.png       %out_path%\icon_point_blue.png  force=%MAX_VALUE color=%BLUE

start %color_exe %source_path%\godot\icon_editor_plugin.png       %out_path%\puzzle_white.png  force=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\godot\icon_editor_plugin.png       %out_path%\puzzle_yellow.png  force=%MAX_VALUE color=%YELLOW

start %color_exe %source_path%\appbar\alert.png                 %out_path%\alert_white.png              max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\appbar\alert.png                 %out_path%\alert_green.png              max=%MAX_VALUE color=%GREEN
start %color_exe %source_path%\appbar\alert.png                 %out_path%\alert_red.png                max=%MAX_VALUE color=%RED
start %color_exe %source_path%\appbar\alert.png                 %out_path%\alert_orange.png             max=%MAX_VALUE color=%ORANGE
start %color_exe %source_path%\appbar\alert.png                 %out_path%\alert_yellow.png             max=%MAX_VALUE color=%YELLOW

start %color_exe %source_path%\appbar\appbar.cabinet.in.png     %out_path%\cabinet_white.png            max=%MAX_VALUE color=%WHITE

start %color_exe %source_path%\appbar\bug.png                   %out_path%\bug_white.png                max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\appbar\bug.png                   %out_path%\bug_yellow.png               max=%MAX_VALUE color=%YELLOW
start %color_exe %source_path%\appbar\bug.png                   %out_path%\bug_orange.png               max=%MAX_VALUE color=%ORANGE

start %color_exe %source_path%\appbar\control.play.png          %out_path%\control_play_white.png       max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\appbar\controller.snes.png       %out_path%\controller_snes_white.png    max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\appbar\debug.step.into.png       %out_path%\debug_step_into_white.png    max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\appbar\debug.step.out.png        %out_path%\debug_step_out_white.png     max=%MAX_VALUE color=%WHITE    
start %color_exe %source_path%\appbar\debug.step.over.png       %out_path%\debug_step_over_white.png    max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\appbar\page.new.png              %out_path%\page_new_white.png           max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\appbar\resource.png              %out_path%\resource_white.png           max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\appbar\tools.png                 %out_path%\tools_white.png               max=%MAX_VALUE color=%WHITE

start %color_exe %source_path%\appbar\appbar.corner.png         %out_path%\expand_white.png               max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\appbar\appbar.page.copy.png       %out_path%\copy_white.png               max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\appbar\appbar.clipboard.paste.png %out_path%\paste_white.png               max=%MAX_VALUE color=%WHITE

start %color_exe %source_path%\appbar\save.png                  %out_path%\save_white.png               max=%MAX_VALUE color=%WHITE
start %color_exe %source_path%\appbar\save.png                  %out_path%\save_orange.png              max=%MAX_VALUE color=%ORANGE
start %color_exe %source_path%\appbar\save.png                  %out_path%\save_blue.png                max=%MAX_VALUE color=%BLUE