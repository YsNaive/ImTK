using ImGuiNET;
class DockTest {
    void Test() {
        uint dockspaceId = 0;
        ImGui.DockBuilderRemoveNode(dockspaceId);
        ImGui.DockBuilderAddNode(dockspaceId);
    }
}
