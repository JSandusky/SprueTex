#pragma once

#include <SprueEngine/MathGeoLib/AllMath.h>
#include <SprueEngine/Libs/Jzon.h>
#include <SprueEngine/GeneralUtility.h>

#include <map>
#include <sstream>

namespace SprueEngine
{


    struct SPRUE VolumePartitioner
    {
        struct SPRUE OcclusionGroup
        {
            BoundingBox bb;
            std::vector<BoundingBox> volumes;

            OcclusionGroup()
            {

            }

            void addVolume(BoundingBox bb)
            {
                if (volumes.size == 0)
                {
                    this->bb = bb;
                }
                else
                {
                    this->bb.Enclose(bb);
                }

                volumes.push_back(bb);
            }

            OcclusionGroup& clear()
            {
                bb.minPoint = Vec3(0, 0, 0);
                bb.maxPoint = Vec3(0, 0, 0);
                volumes.clear();

                return *this;
            }

            void sortY()
            {
                std::sort(volumes.begin(), volumes.end(), [](const BoundingBox& bb0, const BoundingBox& bb1) {
                    return bb0.minPoint.y < bb1.minPoint.y;
                });
            }

            void sortZ()
            {
                std::sort(volumes.begin(), volumes.end(), [](const BoundingBox& bb0, const BoundingBox& bb1) {
                    return bb0.minPoint.z < bb1.minPoint.z;
                });
            }
        };

        struct SPRUE OcclusionArea
        {
            BoundingBox box;
            std::string  name;
            VolumePartitioner* parent = 0x0;

            OcclusionArea(BoundingBox bb, std::string name, VolumePartitioner* parent) :
                box(bb),
                name(name),
                parent(parent)
            {
            }

            BoundingBox intersection(BoundingBox other)
            {
                return box.Intersection(other);
            }
        };

        VolumePartitioner* parent = 0x0;
        std::map<std::string, Jzon::Node> methodTable;
        std::map<std::string, Jzon::Node> tempMethodTable;
        std::vector<OcclusionArea> occluders;

        std::vector<Jzon::Node> ruleStack;

        Jzon::Node Mesh;

        Vec3 min;
        Vec3 max;

        std::vector<VolumePartitioner*> children;

        std::map<std::string, std::string> defines;
        std::map<std::string, float> variables;

        Mat3x4 parentTransform = Mat3x4::identity;
        Vec3 parentPos;
        Mat3x4 localTransform = Mat3x4::identity;
        Mat3x4 composedTransform = Mat3x4::identity;
        Mat3x4 invComposedTransform = Mat3x4::identity;

        Vec3 snap;

        VolumePartitioner(Vec3 min, Vec3 max, Jzon::Node rule, std::map<std::string, Jzon::Node> methodTable, VolumePartitioner* parent, std::vector<OcclusionArea> occluders)
        {
            this->min = min;
            this->max = max;

            this->occluders = occluders;

            if (parent != 0x0)
            {
                this->parentTransform = parent->composedTransform;

                parentPos = (parent->min) + (parent->max * 0.5f);
                Vec3 mc = min + (max * 0.5f);

                Vec3 diff = mc - parentPos;

                this->localTransform.SetTranslatePart(diff);

                for (auto entry : parent->tempMethodTable)
                    tempMethodTable[entry.first] = entry.second;
            }

            composedTransform = (parentTransform) * (localTransform);
            invComposedTransform = (composedTransform).Inverted();

            if (rule.isValid() && rule.getCount() != 0) 
                this->ruleStack.push_back(rule.get(0));
            this->methodTable = methodTable;
            this->parent = parent;

            if (parent != 0x0)
            {
                for (auto entry : parent->defines)
                    defines[entry.first] = entry.second;
                for (auto entry : parent->variables)
                    variables[entry.first] = entry.second;

                snap = parent->snap;
            }
        }

        void transform(Mat3x4 transform)
        {
            localTransform = localTransform * transform;
            composedTransform = parentTransform * localTransform;
            invComposedTransform = composedTransform.Inverted();
        }

        void transformVolume(Mat3x4 transform)
        {
            localTransform = localTransform * transform;
            composedTransform = parentTransform * localTransform;
            invComposedTransform = composedTransform.Inverted();

            Vec3 center = min + (max * 0.5f);

            min -= center;
            max -= center;

            min = transform * min;
            max = transform * max;

            if (max.x < min.x)
                std::swap(max.x, min.x);

            if (max.y < min.y)
                std::swap(max.y, min.y);

            if (max.z < min.z)
                std::swap(max.z, min.z);

            min += center;
            max += center;
        }

        void applyCoords(std::string coords)
        {
            int pos = 0;

            std::string nX = "";
            std::string nY = "";
            std::string nZ = "";
            std::string coord = "";

            for (int i = 0; i < coords.length(); i++)
            {
                std::string c = "" + coords[i];
                coord += c;
                if (EqualsIgnoreCase(c, "X") || EqualsIgnoreCase(c, "Y") || EqualsIgnoreCase(c, "Z"))
                {

                    if (pos == 0)
                    {
                        nX = coord;
                    }
                    else if (pos == 1)
                    {
                        nY = coord;
                    }
                    else if (pos == 2)
                    {
                        nZ = coord;
                    }

                    pos++;
                    coord = "";
                }
            }
            setCoords(nX, nY, nZ);
        }

        void setCoords(std::string X, std::string Y, std::string Z)
        {
            Mat3x4 rotation = Mat3x4::identity;

            float x = X.length() > 1 ? parseEquation(X.substr(0, X.length() - 1), 0, variables) : 0;
            float y = Y.length() > 1 ? parseEquation(Y.substr(0, Y.length() - 1), 0, variables) : 0;
            float z = Z.length() > 1 ? parseEquation(Z.substr(0, Z.length() - 1), 0, variables) : 0;

            if (x != 0) 
                rotation.RotateAxisAngle(Vec3(1, 0, 0), x * DEG_TO_RAD);
            if (y != 0) 
                rotation.RotateAxisAngle(Vec3(0, 1, 0), y * DEG_TO_RAD);
            if (z != 0) 
                rotation.RotateAxisAngle(Vec3(0, 0, 1), z * DEG_TO_RAD);

            transformVolume(rotation);
        }

        void evaluate(float x, float y, float z)
        {
            processResize("X", x);
            processResize("Y", y);
            processResize("Z", z);
            evaluateInternal(this);
        }

        static void evaluateInternal(VolumePartitioner* root)
        {
            std::vector<VolumePartitioner*> processQueue;
            std::vector<VolumePartitioner*> deferQueue;

            processQueue.push_back(root);

            while (processQueue.size() != 0)
            {
                VolumePartitioner* current = processQueue.front();
                while (current != 0x0)
                {
                    if (!current->processRuleStack())
                        deferQueue.push_back(current);

                    for (VolumePartitioner* child : current->children) 
                        processQueue.push_back(child); // Possible performance issue

                    current = processQueue.front();
                }

                std::vector<VolumePartitioner*> temp = processQueue;
                processQueue = deferQueue;
                deferQueue = temp;
            }
        }

        float getVal(std::string axis, Vec3 vals)
        {
            if (axis.length() > 2 || axis.length() == 0) 
                return std::numeric_limits<float>::max();

            if (EqualsIgnoreCase(axis.substr(axis.length() - 1, axis.length()), "X"))
            {
                return vals.x;
            }
            else if (EqualsIgnoreCase(axis.substr(axis.length() - 1, axis.length()), "Y"))
            {
                return vals.y;
            }
            else if (EqualsIgnoreCase(axis.substr(axis.length() - 1, axis.length()), "Z"))
            {
                return vals.z;
            }
            return std::numeric_limits<float>::max();
        }

        void modVal(std::string axis, Vec3 vals, float val)
        {
            if (EqualsIgnoreCase(axis.substr(axis.length() - 1, axis.length()), "X"))
            {
                vals.x += val;
            }
            else if (EqualsIgnoreCase(axis.substr(axis.length() - 1, axis.length()), "Y"))
            {
                vals.y += val;
            }
            else if (EqualsIgnoreCase(axis.substr(axis.length() - 1, axis.length()), "Z"))
            {
                vals.z += val;
            }
        }

        void setVal(std::string axis, Vec3 vals, float val)
        {
            if (EqualsIgnoreCase(axis.substr(axis.length() - 1, axis.length()), "X"))
            {
                vals.x = val;
            }
            else if (EqualsIgnoreCase(axis.substr(axis.length() - 1, axis.length()), "Y"))
            {
                vals.y = val;
            }
            else if (EqualsIgnoreCase(axis.substr(axis.length() - 1, axis.length()), "Z"))
            {
                vals.z = val;
            }
        }

        void repeat(std::string eqn, int repeats, float offset, Jzon::Node ruleOffset, std::string offsetCoord, Jzon::Node ruleSub, std::string ruleCoord, Jzon::Node ruleRemainder, std::string remainderCoord, Jzon::Node repeatRule, std::string axis)
        {
            float interval = getVal(axis, max) - getVal(axis, min);

            Vec3 nmin = min;
            Vec3 nmax = max;

            setVal(axis, nmax, getVal(axis, nmin) + offset);

            if (ruleOffset.isValid())
            {
                VolumePartitioner* vp = new VolumePartitioner(nmin, nmax, ruleOffset, methodTable, this, occluders);
                children.push_back(vp);
                vp->applyCoords(offsetCoord);
            }

            setVal(axis, nmin, getVal(axis, nmax));

            int rep = 0;
            while (true)
            {
                setVal(axis, nmax, getVal(axis, nmin) + parseEquation(eqn, interval, variables));

                if (rep == repeats) break;
                else if (repeats < 0)
                {
                    if (getVal(axis, nmax) > getVal(axis, max)) break;
                }

                VolumePartitioner* vp = new VolumePartitioner(nmin, nmax, ruleSub, methodTable, this, occluders);
                children.push_back(vp);
                vp->applyCoords(ruleCoord);

                rep++;

                setVal(axis, nmin, getVal(axis, nmax));

                if (repeatRule.isValid()) 
                    processRuleBlock(repeatRule);
            }

            if (ruleRemainder.isValid())
            {
                if (getVal(axis, nmin) < getVal(axis, max))
                {
                    setVal(axis, nmax, getVal(axis, max));

                    VolumePartitioner* vp = new VolumePartitioner(nmin, nmax, ruleRemainder, methodTable, this, occluders);
                    children.push_back(vp);
                    vp->applyCoords(remainderCoord);
                }

            }
        }

        void processRepeat(Jzon::Node repeat, std::string axis)
        {
            std::string eqn = repeat.get("Size").toString();
            int repeats = repeat.get("Repeats").toInt(-1);
            float offset = repeat.has("Offset") ? parseEquation(repeat.get("Offset").toString(), getVal(axis, max) - getVal(axis, min), variables) : 0.0f;

            Jzon::Node ruleSub = repeat.get("Rule");
            if (ruleSub.isValid())
            {
                std::string ruleString = repeat.get("Rule").toString();
                auto foundSub = methodTable.find(ruleString);
                if (foundSub != methodTable.end())
                    ruleSub = foundSub->second;
            }
            else
            {
                Jzon::Node temp("TempRule");
                temp.add(child);
                ruleSub = temp;
            }

            std::string ruleCoord = "xyz";
            if (repeat.has("RuleCoord"))
                ruleCoord = repeat.get("RuleCoord").toString();

            Jzon::Node ruleOffset = 0x0;
            std::string offsetCoord = "xyz";
            if (repeat.has("OffsetRule"))
            {
                ruleOffset = repeat.get("OffsetRule");
                if (ruleOffset.isValid())
                {
                    std::string ruleString = repeat.get("OffsetRule").toString();
                    auto foundRule = methodTable.find(ruleString);
                    if (foundRule != methodTable.end())
                        ruleOffset = foundRule->second;
                }
                else
                {
                    Jzon::Node temp("TempRule");
                    temp.add(ruleOffset);
                    ruleOffset = temp;
                }

                if (repeat.has("OffsetCoord"))
                    offsetCoord = repeat.get("OffsetCoord").toString();
            }

            Jzon::Node ruleRemainder;
            std::string remainderCoord = "xyz";
            if (repeat.has("RemainderRule"))
            {
                ruleRemainder = repeat.get("RemainderRule").get(0);
                if (ruleRemainder.isValid())
                {
                    std::string ruleString = repeat.getString("RemainderRule");
                    ruleRemainder = methodTable.get(ruleString);
                    if (ruleRemainder == null) ruleRemainder = tempMethodTable.get(ruleString);
                }
                else
                {
                    Jzon::Node temp("TempRule");
                    temp.name = "TempRule";
                    temp.child = ruleRemainder;
                    ruleRemainder = temp;
                }

                if (repeat.has("RemainderCoord"))
                    remainderCoord = repeat.get("RemainderCoord").toString();
            }

            Jzon::Node ruleRepeat = null;
            if (repeat.has("RepeatRule"))
            {
                ruleRepeat = repeat.get("RepeatRule").child;
                if (ruleRepeat == null)
                {
                    std::string ruleString = repeat.getString("RepeatRule");
                    ruleRepeat = methodTable.get(ruleString);
                    if (ruleRepeat == null) ruleRepeat = tempMethodTable.get(ruleString);
                }
                else
                {
                    Jzon::Node temp = new Jzon::Node("TempRule");
                    temp.add(ruleRepeat);
                    ruleRepeat = temp;
                }
            }

            if (axis != null) 
                repeat(eqn, repeats, offset, ruleOffset, offsetCoord, ruleSub, ruleCoord, ruleRemainder, remainderCoord, ruleRepeat, axis);
        }

        const char[][] csvDelimiters = {
            { '(', ')' }
        };
        
        static std::vector<std::string> parseCSV(std::string csv)
        {
            std::vector<std::string> store;
            std::stringstream builder;
            int delimiter = -1;

            for (int i = 0; i < csv.length(); i++)
            {
                char c = csv[i];
                if (delimiter == -1)
                {
                    if (c == ',')
                    {
                        store.push_back(builder.str());
                        builder.clear();
                    }
                    else
                    {
                        builder << c;

                        for (int d = 0; d < csvDelimiters.length; d++)
                        {
                            if (csvDelimiters[d][0] == c)
                            {
                                delimiter = d;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    builder << c;

                    if (csvDelimiters[delimiter][1] == c)
                    {
                        delimiter = -1;
                    }
                }
            }

            if (builder.tellg() > 0)
                store.push_back(builder.str());

            std::vector<std::string> values;
            for (int i = 0; i < store.size; i++) 
                values[i] = store[i];

            for (int i = 0; i < values.size(); i++)
                values[i] = Trim(values[i]);

            return values;
        }

        void processSplit(Jzon::Node split)
        {
            std::string[] csvs = split.asstd::stringArray();
            std::string[][] splits = new std::string[csvs.length][];

            Jzon::Node ruleRemainder = null;

            for (int i = 0; i < csvs.length; i++)
            {
                splits[i] = parseCSV(csvs[i]);

                if (splits[i][0].equalsIgnoreCase("Remainder"))
                {
                    ruleRemainder = methodTable.get(splits[i][1]);
                    if (ruleRemainder.isValid()) 
                        ruleRemainder = tempMethodTable.get(splits[i][1]);
                }
            }

            BoundingBox bb;

            float x = (max.x - min.x) / 2.0f;
            float y = (max.y - min.y) / 2.0f;
            float z = (max.z - min.z) / 2.0f;

            Vec3 nmin = Pools.obtain(Vec3.class);
            Vec3 nmax = Pools.obtain(Vec3.class);
            Vec3 center = Pools.obtain(Vec3.class).set(min).add(max).scl(0.5f);

            nmin.set(x, y, z).scl(-1);
            nmax.set(x, y, z);
            bb.set(nmin, nmax);

            bb.mul(composedTransform);

            std::vector<BoundingBox> usedVolumes = new std::vector<BoundingBox>(false, 16);

            for (OcclusionArea area : occluders)
            {
                Jzon::Node rule = null;
                for (std::string[] values : splits)
                {
                    if (values[0].equalsIgnoreCase(area.name))
                    {
                        rule = methodTable.get(values[1]);
                        if (rule == null) rule = tempMethodTable.get(values[1]);
                        break;
                    }
                }
                if (rule == null) continue;

                BoundingBox intersection = area.intersection(bb);

                if (intersection != null)
                {
                    intersection.TransformAsAABB(invComposedTransform);

                    nmin = intersection.minPoint + center;
                    nmax = intersection.maxPoint + center;
                    intersection.minPoint = nmin;
                    intersection.maxPoint = nmax;

                    children.push_back(new VolumePartitioner(intersection.min, intersection.max, rule, methodTable, this, occluders));

                    usedVolumes.push_back(intersection);
                }
            }

            std::sort(usedVolumes.begin(), usedVolumes.end(), [](const BoundingBox& lhs, const BoundingBox& rhs) {
                return lhs.minPoint.x < rhs.minPoint.x;
            });

            nmin = min;
            nmax = max;

            if (usedVolumes.size == 0)
            {
                children.push_back(new VolumePartitioner(nmin, nmax, ruleRemainder, methodTable, this, occluders));
            }
            else
            {
                // fill X volumes

                std::vector<OcclusionGroup> groupedx = new std::vector<OcclusionGroup>();

                // Group X's
                for (BoundingBox uv : usedVolumes)
                {
                    OcclusionGroup og = groupedx.size > 0 ? groupedx.peek() : null;
                    if (og != null && uv.max.x >= og.bb.min.x && uv.min.x <= og.bb.max.x)
                    {
                        og.addVolume(uv);
                    }
                    else
                    {
                        og = OcclusionGroup();
                        og.addVolume(uv);
                        groupedx.add(og);
                    }
                }

                nmin = min;
                nmax = max;

                for (OcclusionGroup og : groupedx)
                {
                    og.sortY();

                    nmax.x = og.bb.minPoint.x;

                    if (nmin.x < nmax.x)
                        children.push_back(new VolumePartitioner(nmin, nmax, ruleRemainder, methodTable, this, occluders));

                    nmin.x = og.bb.maxPoint.x;
                }

                nmax.x = max.x;

                if (nmin.x < nmax.x)
                    children.push_back(new VolumePartitioner(nmin, nmax, ruleRemainder, methodTable, this, occluders));

                // Fill Y volumes

                std::vector<std::vector<OcclusionGroup>> groupedy;

                // Group Y's
                for (OcclusionGroup ogx : groupedx)
                {
                    groupedy.push_back(std::vector<OcclusionGroup>(false, 16));
                    for (BoundingBox uv : ogx.volumes)
                    {
                        OcclusionGroup og = groupedy.peek().size > 0 ? groupedy.peek().peek() : null;
                        if (og != null && uv.max.y >= og.bb.min.y && uv.min.y <= og.bb.max.y)
                        {
                            og.addVolume(uv);
                        }
                        else
                        {
                            og = Pools.obtain(OcclusionGroup.class).clear();
                            og.addVolume(uv);
                            groupedy.peek().add(og);
                        }
                    }
                }

                for (Array<OcclusionGroup> group : groupedy)
                {
                    nmin.set(min);
                    nmax.set(max);

                    for (OcclusionGroup og : group)
                    {
                        og.sortZ();
                        nmin.x = og.bb.min.x;
                        nmax.x = og.bb.max.x;

                        nmax.y = og.bb.min.y;

                        if (nmin.y < nmax.y)
                        {
                            children.add(new VolumePartitioner(nmin, nmax, ruleRemainder, methodTable, this, occluders));
                        }

                        nmin.y = og.bb.max.y;
                    }

                    nmax.y = max.y;

                    if (nmin.y < nmax.y)
                    {
                        children.add(new VolumePartitioner(nmin, nmax, ruleRemainder, methodTable, this, occluders));
                    }
                }

                // Fill Z volumes

                Array<Array<Array<OcclusionGroup>>> groupedz = new Array<Array<Array<OcclusionGroup>>>();

                // Group Z's
                for (Array<OcclusionGroup> groupy : groupedy)
                {
                    groupedz.add(new Array<Array<OcclusionGroup>>(false, 16));
                    for (OcclusionGroup ogy : groupy)
                    {
                        groupedz.peek().add(new Array<OcclusionGroup>(false, 16));
                        for (BoundingBox uv : ogy.volumes)
                        {
                            OcclusionGroup og = groupedz.peek().peek().size > 0 ? groupedz.peek().peek().peek() : null;
                            if (og != null && uv.max.z >= og.bb.min.z && uv.min.z <= og.bb.max.z)
                            {
                                og.addVolume(uv);
                            }
                            else
                            {
                                og = Pools.obtain(OcclusionGroup.class).clear();
                                og.addVolume(uv);
                                groupedz.peek().peek().add(og);
                            }
                        }
                    }
                }

                for (Array<Array<OcclusionGroup>> groupx : groupedz)
                {
                    for (Array<OcclusionGroup> groupy : groupx)
                    {
                        nmin.set(min);
                        nmax.set(max);

                        for (OcclusionGroup og : groupy)
                        {
                            og.sortZ();
                            nmin.x = og.bb.min.x;
                            nmax.x = og.bb.max.x;

                            nmax.z = og.bb.min.z;

                            if (nmin.z < nmax.z)
                            {
                                children.add(new VolumePartitioner(nmin, nmax, ruleRemainder, methodTable, this, occluders));
                            }

                            nmin.z = og.bb.max.z;
                        }

                        nmax.z = max.z;

                        if (nmin.z < nmax.z)
                        {
                            children.add(new VolumePartitioner(nmin, nmax, ruleRemainder, methodTable, this, occluders));
                        }
                    }
                }

                for (OcclusionGroup og : groupedx)
                {
                    og.clear();
                    Pools.free(og);
                }

                for (Array<OcclusionGroup> groupy : groupedy)
                {
                    for (OcclusionGroup og : groupy)
                    {
                        og.clear();
                        Pools.free(og);
                    }
                }

                for (Array<Array<OcclusionGroup>> groupx : groupedz)
                {
                    for (Array<OcclusionGroup> groupy : groupx)
                    {
                        for (OcclusionGroup og : groupy)
                        {
                            og.clear();
                            Pools.free(og);
                        }
                    }
                }
            }

            Pools.free(bb);
            Pools.free(nmin);
            Pools.free(nmax);

            for (BoundingBox uv : usedVolumes) Pools.free(uv);
        }

        void processDivide(Jzon::Node divide, std::string axis)
        {
            float interval = getVal(axis, max) - getVal(axis, min);

            Vec3 nmin = min;
            Vec3 nmax = max;

            std::string[] csvs = divide.asstd::stringArray();

            for (int i = 0; i < csvs.length; i++)
            {
                std::string[] values = parseCSV(csvs[i]);

                std::string eqn = values[0];
                std::string ruleString = values[1];
                std::string coords = values.length > 2 ? values[2] : null;

                float size = parseEquation(eqn, interval, variables);

                setVal(axis, nmax, getVal(axis, nmin) + size);

                Jzon::Node rule = methodTable.get(ruleString);
                if (rule == null) rule = tempMethodTable.get(ruleString);
                if (rule == null) throw new RuntimeException("Rule: " + ruleString + " does not exist!");

                VolumePartitioner vp = new VolumePartitioner(nmin, nmax, rule, methodTable, this, occluders);
                if (coords != null) vp.applyCoords(coords);
                children.add(vp);

                setVal(axis, nmin, getVal(axis, nmax));
            }
        }

        public void processSelect(Jzon::Node select)
        {
            Vec3 nmin = Pools.obtain(Vec3.class);
            Vec3 nmax = Pools.obtain(Vec3.class);

            Vec3 rmin = Pools.obtain(Vec3.class);
            Vec3 rmax = Pools.obtain(Vec3.class);

            rmin.set(min);
            rmax.set(max);

            std::string[] csvs = select.asstd::stringArray();
            std::string remainder = null;

            for (int i = 0; i < csvs.length; i++)
            {
                std::string[] values = parseCSV(csvs[i]);

                std::string side = values[0];

                if (side.equalsIgnoreCase("Remainder"))
                {
                    remainder = csvs[i];
                    continue;
                }

                std::string eqn = values[1];
                std::string ruleString = values[2];
                std::string coords = values.length > 3 ? values[3] : null;

                std::string axis = "";
                if (side.equalsIgnoreCase("left") || side.equalsIgnoreCase("right")) axis = "X";
                else if (side.equalsIgnoreCase("top") || side.equalsIgnoreCase("bottom")) axis = "Y";
                else if (side.equalsIgnoreCase("front") || side.equalsIgnoreCase("back")) axis = "Z";
                else throw new RuntimeException("Invalid side: " + side);

                float interval = getVal(axis, max) - getVal(axis, min);
                float size = parseEquation(eqn, interval, variables);

                nmin.set(rmin);
                nmax.set(rmax);

                if (side.equalsIgnoreCase("left"))
                {
                    nmax.x = nmin.x + size;
                    rmin.x = nmax.x;
                }
                else if (side.equalsIgnoreCase("right"))
                {
                    nmin.x = nmax.x - size;
                    rmax.x = nmin.x;
                }
                else if (side.equalsIgnoreCase("bottom"))
                {
                    nmax.y = nmin.y + size;
                    rmin.y = nmax.y;
                }
                else if (side.equalsIgnoreCase("top"))
                {
                    nmin.y = nmax.y - size;
                    rmax.y = nmin.y;
                }
                else if (side.equalsIgnoreCase("back"))
                {
                    nmax.z = nmin.z + size;
                    rmin.z = nmax.z;
                }
                else if (side.equalsIgnoreCase("front"))
                {
                    nmin.z = nmax.z - size;
                    rmax.z = nmin.z;
                }

                Jzon::Node rule = methodTable.get(ruleString);
                if (rule == null) rule = tempMethodTable.get(ruleString);
                VolumePartitioner vp = new VolumePartitioner(nmin, nmax, rule, methodTable, this, occluders);
                if (coords != null) vp.applyCoords(coords);
                children.add(vp);
            }

            if (remainder != null)
            {
                std::string[] values = parseCSV(remainder);

                std::string ruleString = values[1];
                std::string coords = values.length > 2 ? values[2] : null;

                Jzon::Node rule = methodTable.get(ruleString);
                if (rule == null) rule = tempMethodTable.get(ruleString);
                VolumePartitioner vp = new VolumePartitioner(rmin, rmax, rule, methodTable, this, occluders);
                if (coords != null) vp.applyCoords(coords);
                children.add(vp);
            }

            Pools.free(nmin);
            Pools.free(nmax);

            Pools.free(rmin);
            Pools.free(rmax);
        }

        public void processResize(std::string axis, float val)
        {
            Vec3 lastPos = Pools.obtain(Vec3.class).set(min).add(max).scl(0.5f);

            float interval = getVal(axis, max) - getVal(axis, min);

            val /= 2.0f;

            int snapVal = MathUtils.round(getVal(axis, snap));
            if (snapVal == 1)
            {
                setVal(axis, min, getVal(axis, max) - (val * 2));
            }
            else if (snapVal == 0)
            {
                setVal(axis, min, interval / 2.0f + getVal(axis, min));
                setVal(axis, max, getVal(axis, min));

                modVal(axis, min, -val);
                modVal(axis, max, val);
            }
            else if (snapVal == -1)
            {
                setVal(axis, max, getVal(axis, min) + (val * 2));
            }
            else throw new RuntimeException("Invalid snap val: " + snap + " for axis: " + axis);

            Vec3 newPos = Pools.obtain(Vec3.class).set(min).add(max).scl(0.5f);
            Vec3 diff = newPos.sub(lastPos);
            this.localTransform.translate(diff);
            Pools.free(lastPos);
            Pools.free(newPos);
            composedTransform.set(parentTransform).mul(localTransform);
            invComposedTransform.set(composedTransform).inv();
        }

        public void processMultiConditional(Jzon::Node conditional, boolean canInterrupt)
        {
            Jzon::Node current = conditional.child;
            while (current != null)
            {
                std::string[] conditions = parseCSV(current.name);
                boolean pass = true;

                if (!current.name.equalsIgnoreCase("else") && conditions.length > 0)
                {
                    for (std::string condition : conditions)
                    {
                        if (!evaluateConditional(condition))
                        {
                            pass = false;
                            break;
                        }
                    }
                }

                if (pass)
                {
                    Jzon::Node rule = current.child;
                    if (rule == null)
                    {
                        std::string ruleString = current.asstd::string();
                        rule = methodTable.get(ruleString);
                        if (rule == null) rule = tempMethodTable.get(ruleString);
                    }
                    else
                    {
                        Jzon::Node temp = new Jzon::Node("TempRule");
                        temp.name = "TempRule";
                        temp.child = rule;
                        rule = temp;
                    }

                    if (rule.child != null)
                    {
                        if (canInterrupt) ruleStack.addFirst(rule.child);
                        else processRuleBlock(rule);
                    }
                    break;
                }

                current = current.next;
            }

        }

        private static final std::string[] validComparisons = { "<=", ">=", "==", "<", ">" };
        private boolean evaluateConditional(std::string condition)
        {
            condition = condition.trim();

            boolean succeed = false;

            if (condition.startsWith("occluded("))
            {
                std::string name = condition.substr(8, condition.length() - 1);

                float x = (max.x - min.x) / 2.0f;
                float y = (max.y - min.y) / 2.0f;
                float z = (max.z - min.z) / 2.0f;

                BoundingBox bb = Pools.obtain(BoundingBox.class);
                Vec3 tmin = Pools.obtain(Vec3.class);
                Vec3 tmax = Pools.obtain(Vec3.class);

                tmin.set(x, y, z).scl(-1);
                tmax.set(x, y, z);
                bb.set(tmin, tmax);
                bb.mul(composedTransform);

                succeed = false;
                for (OcclusionArea oa : occluders)
                {
                    if (oa.name.equalsIgnoreCase(name) && bb.intersects(oa.box))
                    {
                        succeed = true;
                        break;
                    }
                }

                Pools.free(bb);
                Pools.free(tmin);
                Pools.free(tmax);
            }
            else if (condition.startsWith("defined("))
            {
                std::string name = condition.substr(8, condition.length() - 1);
                if (variables.containsKey(name) || defines.containsKey(name))
                {
                    succeed = true;
                }
            }
            else
            {
                boolean found = false;

                for (std::string comparison : validComparisons)
                {
                    if (condition.contains(comparison))
                    {
                        found = true;

                        std::string[] parts = condition.split(comparison);
                        float left = parseEquation(parts[0], 0, variables);
                        float right = parseEquation(parts[1], 0, variables);

                        if (comparison.equals("<="))
                        {
                            succeed = left <= right;
                        }
                        else if (comparison.equals(">="))
                        {
                            succeed = left >= right;
                        }
                        else if (comparison.equals("=="))
                        {
                            succeed = left == right;
                        }
                        else if (comparison.equals("<"))
                        {
                            succeed = left < right;
                        }
                        else if (comparison.equals(">"))
                        {
                            succeed = left > right;
                        }

                        break;
                    }
                }

                if (!found) throw new RuntimeException("Invalid conditional: " + condition);
            }

            return succeed;
        }

        public void processOcclude(Jzon::Node occlude)
        {
            std::string xeqn = occlude.getString("X", "100%");
            std::string yeqn = occlude.getString("Y", "100%");
            std::string zeqn = occlude.getString("Z", "100%");

            std::string oxeqn = occlude.getString("OX", "0");
            std::string oyeqn = occlude.getString("OY", "0");
            std::string ozeqn = occlude.getString("OZ", "0");

            float dx = max.x - min.x;
            float dy = max.y - min.y;
            float dz = max.z - min.z;

            float x = parseEquation(xeqn, dx, variables) / 2.0f;
            float y = parseEquation(yeqn, dy, variables) / 2.0f;
            float z = parseEquation(zeqn, dz, variables) / 2.0f;

            float ox = parseEquation(oxeqn, dx, variables) / 2.0f;
            float oy = parseEquation(oyeqn, dy, variables) / 2.0f;
            float oz = parseEquation(ozeqn, dz, variables) / 2.0f;

            BoundingBox bb = Pools.obtain(BoundingBox.class);
            Vec3 tmin = Pools.obtain(Vec3.class);
            Vec3 tmax = Pools.obtain(Vec3.class);

            dx /= 2;
            dy /= 2;
            dz /= 2;

            if (snap.x == -1)
            {
                tmin.x = -dx + ox;
                tmax.x = -dx + x * 2 + ox;
            }
            else if (snap.x == 1)
            {
                tmin.x = dx - x * 2 + ox;
                tmax.x = dx + ox;
            }
            else
            {
                tmin.x = -x + ox;
                tmax.x = x + ox;
            }

            if (snap.y == -1)
            {
                tmin.y = -dy + oy;
                tmax.y = -dy + y * 2 + oy;
            }
            else if (snap.y == 1)
            {
                tmin.y = dy - y * 2 + oy;
                tmax.y = dy + oy;
            }
            else
            {
                tmin.y = -y + oy;
                tmax.y = y + oy;
            }

            if (snap.z == -1)
            {
                tmin.z = -dz + oz;
                tmax.z = -dz + z * 2 + oz;
            }
            else if (snap.z == 1)
            {
                tmin.z = dz - z * 2 + oz;
                tmax.z = dz + oz;
            }
            else
            {
                tmin.z = -z + oz;
                tmax.z = z + oz;
            }

            bb.set(tmin, tmax);
            bb.mul(composedTransform);

            std::string name = occlude.getString("Name", "");

            occluders.add(new OcclusionArea(bb, name, this));

            Pools.free(tmin);
            Pools.free(tmax);
        }

        public boolean processRuleStack()
        {
            while (true)
            {
                Jzon::Node current = ruleStack.pollFirst();

                if (current == null)
                {
                    break;
                }

                if (current.next != null) ruleStack.addFirst(current.next);

                if (!processRule(current, true))
                {
                    return false;
                }
            }
            return true;
        }

        public void processRuleBlock(Jzon::Node rule)
        {
            Jzon::Node current = rule.child;
            while (current != null)
            {
                processRule(current, false);
                current = current.next;
            }
        }

        public boolean processRule(Jzon::Node current, boolean canInterrupt)
        {
            std::string method = current.name;

            if (method.equalsIgnoreCase("Rule"))
            {
                Jzon::Node nrule = methodTable.get(current.asstd::string());
                if (nrule == null) throw new RuntimeException("Invalid rule: " + current.asstd::string());
                if (nrule.child != null) ruleStack.addFirst(nrule.child);
            }
            else if (method.equalsIgnoreCase("Child"))
            {
                Jzon::Node rule = current.child;
                if (rule == null)
                {
                    std::string ruleString = current.asstd::string();
                    rule = methodTable.get(ruleString);
                    if (rule == null) rule = tempMethodTable.get(ruleString);
                }
                else
                {
                    Jzon::Node temp = new Jzon::Node("TempRule");
                    temp.name = "TempRule";
                    temp.child = rule;
                    rule = temp;
                }

                VolumePartitioner vp = new VolumePartitioner(min, max, rule, methodTable, this, occluders);
                children.add(vp);
            }
            else if (method.equalsIgnoreCase("CoordinateSystem"))
            {
                applyCoords(current.asstd::string());
            }
            else if (method.equalsIgnoreCase("Move"))
            {
                std::string xstring = current.getString("X", "0");
                std::string ystring = current.getString("Y", "0");
                std::string zstring = current.getString("Z", "0");

                float x = parseEquation(xstring, max.x - min.x, variables);
                float y = parseEquation(ystring, max.y - min.y, variables);
                float z = parseEquation(zstring, max.z - min.z, variables);

                Mat3x4 trans = Pools.obtain(Mat3x4.class).setToTranslation(x, y, z);

                transformVolume(trans);

                Pools.free(trans);
            }
            else if (method.equalsIgnoreCase("Rotate"))
            {
                std::string xstring = current.getString("X", "0");
                std::string ystring = current.getString("Y", "1");
                std::string zstring = current.getString("Z", "0");
                std::string astring = current.getString("Angle", "0");

                float x = parseEquation(xstring, 0, variables);
                float y = parseEquation(ystring, 0, variables);
                float z = parseEquation(zstring, 0, variables);
                float a = parseEquation(astring, 0, variables);

                Mat3x4 rotation = Pools.obtain(Mat3x4.class).setToRotation(x, y, z, a);

                transform(rotation);

                Pools.free(rotation);

            }
            else if (method.equalsIgnoreCase("MultiConditional"))
            {
                processMultiConditional(current, canInterrupt);
            }
            else if (method.equalsIgnoreCase("Split"))
            {
                processSplit(current);
            }
            else if (method.equalsIgnoreCase("Select"))
            {
                processSelect(current);
            }
            else if (method.equalsIgnoreCase("RepeatX"))
            {
                processRepeat(current, "X");
            }
            else if (method.equalsIgnoreCase("RepeatY"))
            {
                processRepeat(current, "Y");
            }
            else if (method.equalsIgnoreCase("RepeatZ"))
            {
                processRepeat(current, "Z");
            }
            else if (method.equalsIgnoreCase("DivideX"))
            {
                processDivide(current, "X");
            }
            else if (method.equalsIgnoreCase("DivideY"))
            {
                processDivide(current, "Y");
            }
            else if (method.equalsIgnoreCase("DivideZ"))
            {
                processDivide(current, "Z");
            }
            else if (method.equalsIgnoreCase("Snap"))
            {
                snap.set(current.getFloat("X", snap.x), current.getFloat("Y", snap.y), current.getFloat("Z", snap.z));
            }
            else if (method.equalsIgnoreCase("Resize"))
            {
                std::string axis = "X";
                std::string eqnstd::string = current.getString("X", "100%");
                float interval = getVal(axis, max) - getVal(axis, min);

                float val = parseEquation(eqnstd::string, interval, variables);

                processResize(axis, val);

                axis = "Y";
                eqnstd::string = current.getString("Y", "100%");
                interval = getVal(axis, max) - getVal(axis, min);

                val = parseEquation(eqnstd::string, interval, variables);

                processResize(axis, val);

                axis = "Z";
                eqnstd::string = current.getString("Z", "100%");
                interval = getVal(axis, max) - getVal(axis, min);

                val = parseEquation(eqnstd::string, interval, variables);

                processResize(axis, val);
            }
            else if (method.equalsIgnoreCase("Define"))
            {
                Jzon::Node definition = current.child;
                while (definition != null)
                {
                    std::string name = definition.name;
                    std::string value = definition.asstd::string();
                    defines.put(name, value);
                    try
                    {
                        double val = parseEquationWithException(value, 0, variables);
                        variables.put(name, val);
                    }
                    catch (Exception e) {}

                    definition = definition.next;
                }
            }
            else if (method.equalsIgnoreCase("Occlude"))
            {
                processOcclude(current);
            }
            else if (method.equalsIgnoreCase("Mesh"))
            {
                Mesh = current;
            }
            else if (method.equalsIgnoreCase("Defer"))
            {
                return false;
            }
            else if (method.startsWith("TempRule"))
            {
                tempMethodTable.put(method, current);
            }
            else if (method.equalsIgnoreCase("GraphData"))
            {

            }
            else
            {
                throw new RuntimeException("Unrecognised Rule: " + method);
            }
            return true;
        }

        public ModelBatchData getModelBatchData()
        {
            Jzon::Node meshValue = Mesh;
            std::string meshName = meshValue.getString("Name");
            if (defines.containsKey(meshName)) meshName = defines.get(meshName);
            std::string textureName = meshValue.getString("Texture");
            boolean useTriplanarSampling = meshValue.has("TriplanarScale");
            float triplanarScale = 0;
            if (useTriplanarSampling) triplanarScale = meshValue.getFloat("TriplanarScale");
            boolean seamless = meshValue.getBoolean("Seamless", true);

            if (defines.containsKey(textureName)) textureName = defines.get(textureName);
            std::string mbname = "";

            if (meshName.equalsIgnoreCase("Sphere"))
            {
                mbname = meshName + textureName + "Theta" + meshValue.getString("Theta", "8") + "Phi" + meshValue.getString("Phi", "8") + useTriplanarSampling + triplanarScale + seamless;
            }
            else if (meshName.equalsIgnoreCase("HemiSphere"))
            {
                mbname = meshName + textureName + "Theta" + meshValue.getString("Theta", "8") + "Phi" + meshValue.getString("Phi", "8") + useTriplanarSampling + triplanarScale + seamless;
            }
            else if (meshName.equalsIgnoreCase("Cylinder"))
            {
                mbname = meshName + textureName + "Phi" + meshValue.getString("Phi", "8") + "HollowScale" + meshValue.getString("HollowScale", "0") + useTriplanarSampling + triplanarScale + seamless;
            }
            else if (meshName.equalsIgnoreCase("Box"))
            {
                std::string eqnx = meshValue.getString("loftX", "100%");
                std::string eqnz = meshValue.getString("loftZ", "100%");
                int snapx = meshValue.getInt("snapX", 0);
                int snapz = meshValue.getInt("snapZ", 0);
                mbname = meshName + textureName + "SnapX" + snapx + "SnapZ" + snapz + "LoftX" + eqnx + "LoftZ" + eqnz + useTriplanarSampling + triplanarScale + seamless;
            }
            else
            {
                mbname = meshName + textureName + useTriplanarSampling + triplanarScale + seamless;
            }

            ModelBatchData data = FileUtils.loadModelBatchData(mbname);
            if (data == null)
            {
                Mesh mesh = null;
                if (meshName.equalsIgnoreCase("Box"))
                {
                    std::string eqnx = meshValue.getString("loftX", "100%");
                    std::string eqnz = meshValue.getString("loftZ", "100%");

                    float loftx = eqnx.equalsIgnoreCase("100%") ? 1 : parseEquation(eqnx, 1, variables);
                    float loftz = eqnz.equalsIgnoreCase("100%") ? 1 : parseEquation(eqnz, 1, variables);

                    int snapx = meshValue.getInt("snapX", 0);
                    int snapz = meshValue.getInt("snapZ", 0);

                    mesh = Shapes.getBoxMesh(1, loftx, snapx, 1, 1, loftz, snapz, true, !useTriplanarSampling);
                }
                else if (meshName.equalsIgnoreCase("Cylinder"))
                {
                    int phi = meshValue.getInt("Phi", 8);

                    std::string eqns = meshValue.getString("HollowScale", "0");
                    float scale = eqns.equalsIgnoreCase("0") ? 0 : parseEquation(eqns, 1, variables);

                    mesh = Shapes.getCylinderMesh(phi, scale>0, scale, true, !useTriplanarSampling);
                }
                else if (meshName.equalsIgnoreCase("Sphere"))
                {
                    int theta = meshValue.getInt("Theta", 8);
                    int phi = meshValue.getInt("Phi", 8);
                    mesh = Shapes.getSphereMesh(theta, phi, 1, true, !useTriplanarSampling);
                }
                else if (meshName.equalsIgnoreCase("HemiSphere"))
                {
                    int theta = meshValue.getInt("Theta", 8);
                    int phi = meshValue.getInt("Phi", 8);
                    mesh = Shapes.getHemiSphereMesh(theta, phi, 1, true, !useTriplanarSampling);
                }
                else
                {
                    mesh = FileUtils.loadMesh(meshName);
                }

                TextureWrap wrap = seamless ? TextureWrap.Repeat : TextureWrap.MirroredRepeat;
                Texture[] textures = FileUtils.getTextureGroup(new std::string[]{ textureName }, wrap);

                data = new ModelBatchData(mesh, GL20.GL_TRIANGLES, textures, false, false, useTriplanarSampling, triplanarScale);
                FileUtils.storeModelBatchData(mbname, data);
            }
            return data;
        }

        private void collectMeshesInternal(Entity entity, OcttreeEntry<Entity> entry, btTriangleMesh triangleMesh)
        {
            if (Mesh != null)
            {
                ModelBatchData data = getModelBatchData();
                ModelBatchInstance mb = new ModelBatchInstance(data);
                BoundingBox bb = Pools.obtain(BoundingBox.class);
                mb.getMesh().calculateBoundingBox(bb);

                Vec3 meshDim = bb.getDimensions();
                meshDim.set(1.0f / meshDim.x, 1.0f / meshDim.y, 1.0f / meshDim.z);
                Vec3 volumeDim = Pools.obtain(Vec3.class).set(max).sub(min);

                Vec3 scale = volumeDim.scl(meshDim);

                Mat3x4 transform = Pools.obtain(Mat3x4.class).idt().mul(this.composedTransform).scale(scale.x, scale.y, scale.z);

                entity.addRenderable(mb, transform);

                Pools.free(bb);
                Pools.free(transform);
                Pools.free(volumeDim);

                if (entry != null)
                {
                    if (entry.box.pos.x - entry.box.extents.x > min.x) entry.box.extents.x = entry.box.pos.x - min.x;
                    if (entry.box.pos.y - entry.box.extents.y > min.y) entry.box.extents.y = entry.box.pos.y - min.y;
                    if (entry.box.pos.z - entry.box.extents.z > min.z) entry.box.extents.z = entry.box.pos.z - min.z;

                    if (entry.box.pos.x + entry.box.extents.x < max.x) entry.box.extents.x = max.x - entry.box.pos.x;
                    if (entry.box.pos.y + entry.box.extents.y < max.y) entry.box.extents.y = max.y - entry.box.pos.y;
                    if (entry.box.pos.z + entry.box.extents.z < max.z) entry.box.extents.z = max.z - entry.box.pos.z;
                }
                if (triangleMesh != null)
                {
                    BulletWorld.addTriangles(mb.getMesh(), transform, triangleMesh);
                }
            }

            for (VolumePartitioner vp : children)
            {
                vp.collectMeshesInternal(entity, entry, triangleMesh);
            }
        }

        public void collectMeshes(Entity entity, OcttreeEntry<Entity> entry, btTriangleMesh triangleMesh)
        {
            collectMeshesInternal(entity, entry, triangleMesh);
        }

        public static void loadImportsAndBuildMethodTable(Array<std::string> importedFiles, Jzon::Node root, HashMap<std::string, Jzon::Node> methodTable, std::string fileName, HashMap<std::string, std::string> renameTable, boolean addMain)
        {
            Jzon::Node imports = root.get("Imports");
            if (imports != null)
            {
                std::string[] files = imports.asstd::stringArray();
                for (std::string file : files)
                {
                    if (!importedFiles.contains(file, false))
                    {
                        importedFiles.add(file);

                        std::string nfileName = file.substr(file.lastIndexOf("/") + 1);
                        nfileName = nfileName.substr(0, nfileName.lastIndexOf(".") + 1);

                        std::string contents = Gdx.files.internal(file).readstd::string();
                        Jzon::Node nroot = new JsonReader().parse(contents);

                        loadImportsAndBuildMethodTable(importedFiles, nroot, methodTable, nfileName, renameTable, false);
                    }
                }
            }

            Jzon::Node current = root.child;
            while (current != null)
            {
                if (current.name.equalsIgnoreCase("Main"))
                {
                    if (addMain)
                    {
                        methodTable.put(current.name, current);
                    }
                }
                else if (current.name.equalsIgnoreCase("Imports"))
                {

                }
                else
                {
                    methodTable.put(fileName + current.name, current);

                    renameTable.put(current.name, fileName + current.name);
                }
                current = current.next;
            }

            correctRenames(root, renameTable);
            renameTable.clear();
        }

        static void correctRenames(Jzon::Node current, HashMap<std::string, std::string> renameTable)
        {
            if (current.isstd::string())
            {
                std::string cstd::string = current.asstd::string();
                if (cstd::string != null)
                {
                    if (renameTable.containsKey(cstd::string))
                    {
                        current.set(renameTable.get(cstd::string));
                    }
                    else
                    {
                        std::string[] split = parseCSV(cstd::string);
                        boolean change = false;
                        for (int i = 0; i < split.length; i++)
                        {
                            if (renameTable.containsKey(split[i]))
                            {
                                split[i] = renameTable.get(split[i]);
                                change = true;
                            }
                        }

                        if (change)
                        {
                            std::string combined = split[0];
                            for (int i = 1; i < split.length; i++)
                            {
                                combined += "," + split[i];
                            }
                            current.set(combined);
                        }
                    }
                }
            }
            if (current->child != 0x0) 
                correctRenames(current.child, renameTable);
            if (current->next != 0x0) 
                correctRenames(current.next, renameTable);
        }

        static VolumePartitioner* load(std::string file)
        {
            std::map<std::string, Jzon::Node> methodTable = FileUtils.loadGrammar(file);
            Jzon::Node main = methodTable.get("Main");

            return new VolumePartitioner(new Vec3(), new Vec3(), main, methodTable, null, new Array<OcclusionArea>(false, 16));
        }

        float parseEquationWithException(std::string equation, float interval, HashMap<std::string, Double> variables) throws UnknownFunctionException, UnparsableExpressionException
        {
            equation = equation.replace("%", "#");

            float size = 0;

            if (percentOperator == null)
            {
                percentOperator = new PercentOperator();
            }
            percentOperator.interval = interval;

            if (rndFunc == null)
            {
                try
                {
                    rndFunc = new CustomFunction("rnd"){
                        public double applyFunction(double... value)
                    {
                        double val = 0;
                        for (double v : value) val += MathUtils.random()*v;
                        return val;
                    }

                    };
                }
                catch (InvalidCustomFunctionException e)
                {
                    e.printStackTrace();
                }
            }

            if (modFunc == null)
            {
                try
                {
                    modFunc = new CustomFunction("mod", 2){
                        public double applyFunction(double... value)
                    {
                        return value[0] % value[1];
                    }

                    };
                }
                catch (InvalidCustomFunctionException e)
                {
                    e.printStackTrace();
                }
            }

            ExpressionBuilder expBuilder = new ExpressionBuilder(equation);
            expBuilder.withCustomFunction(rndFunc);
            expBuilder.withCustomFunction(modFunc);
            expBuilder.withOperation(percentOperator);
            expBuilder.withVariables(variables);

            expBuilder.withVariable("X", max.x - min.x);
            expBuilder.withVariable("Y", max.y - min.y);
            expBuilder.withVariable("Z", max.z - min.z);

            expBuilder.withVariable("x", (max.x - min.x) / 100.0f);
            expBuilder.withVariable("y", (max.y - min.y) / 100.0f);
            expBuilder.withVariable("z", (max.z - min.z) / 100.0f);

            Calculable eqn = expBuilder.build();
            size = (float)eqn.calculate();

            return size;
        }

        float parseEquation(std::string equation, float interval, HashMap<std::string, Double> variables)
        {
            float size = 0;

            try
            {
                size = parseEquationWithException(equation, interval, variables);
            }
            catch (Exception e)
            {
                e.printStackTrace();
                throw new RuntimeException("Error parsing equation: " + equation);
            }

            return size;
        }

        static CustomFunction rndFunc;
        static CustomFunction modFunc;
        static PercentOperator percentOperator;

        static class PercentOperator extends CustomOperator
        {
            float interval;
            protected PercentOperator()
            {
                super("#", true, 1, 1);
            }

                protected double applyOperation(double[] arg0)
            {
                return (arg0[0] / 100.0) * interval;
            }

        }
    };

}