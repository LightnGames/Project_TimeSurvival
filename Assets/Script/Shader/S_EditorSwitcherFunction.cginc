#ifndef EDITOR_SWITCHER_FUNCTION_INCLUDED
#define EDITOR_SWITCHER_FUNCTION_INCLUDED

void EditorSwitcher_half(in half4 Editor, in half4 Player, out half4 Out)
{
#if UNITY_EDITOR
    Out = Editor;
#else
    Out = Player;
#endif
}

#endif